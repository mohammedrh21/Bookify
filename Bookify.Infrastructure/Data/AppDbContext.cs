using Bookify.Domain.Entities;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Bookify.Infrastructure.Data
{
    public class AppDbContext
    : IdentityDbContext<ApplicationIdentityUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Domain.Entities.Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<ContactInfo> ContactInfo { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ServiceApprovalRequest> ServiceApprovalRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ================================
            // Service have convension (price)
            // ================================
            builder.Entity<Domain.Entities.Service>(entity =>
            {
                entity.Property(s => s.Price)
                      .HasPrecision(18, 2); // precision 18, scale 2
            });

            // ================================
            // Client → Booking (One-to-Many)
            // ================================
            builder.Entity<Booking>()
                .HasOne(b => b.Client)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================================
            // Service → Booking (One-to-Many)
            // ================================
            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================================
            // Staff → Service (One-to-One)
            // ================================
            builder.Entity<Domain.Entities.Service>()
                .HasOne(s => s.Staff)
                .WithOne(st => st.Service)
                .HasForeignKey<Domain.Entities.Service>(s => s.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // Category → Service (One-to-Many)
            // ================================
            builder.Entity<Domain.Entities.Service>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // Review Configurations
            // ================================
            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Service)
                    .WithMany(s => s.Reviews)
                    .HasForeignKey(r => r.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Client)
                    .WithMany()
                    .HasForeignKey(r => r.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Booking)
                    .WithOne(b => b.Review)
                    .HasForeignKey<Review>(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================
            // ServiceApprovalRequest Configurations
            // ================================
            builder.Entity<ServiceApprovalRequest>(entity =>
            {
                entity.HasOne(r => r.Staff)
                    .WithMany()
                    .HasForeignKey(r => r.StaffId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Service)
                    .WithMany()
                    .HasForeignKey(r => r.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Actioner)
                    .WithMany()
                    .HasForeignKey(r => r.ActionedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================================
            // Payment Configurations
            // ================================
            builder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                      .HasPrecision(18, 2);

                entity.HasOne(p => p.Client)
                    .WithMany()
                    .HasForeignKey(p => p.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Service)
                    .WithMany()
                    .HasForeignKey(p => p.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Booking)
                    .WithOne(b => b.Payment)
                    .HasForeignKey<Payment>(p => p.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ================================
            // Notification Configurations
            // ================================
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(n => new { n.UserId, n.CreatedAt })
                    .HasDatabaseName("IX_Notifications_UserId_CreatedAt");
            });

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            await UpdateServiceRatingsAsync();
            return result;
        }

        private async Task UpdateServiceRatingsAsync()
        {
            // 1. Identify which services need an update, only if the Rating field was changed
            var serviceIds = ChangeTracker.Entries<Review>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || 
                           (e.State == EntityState.Modified && e.Property(r => r.Rating).IsModified))
                .Select(e => e.Entity.ServiceId)
                .Distinct()
                .ToList();

            if (!serviceIds.Any()) return;

            // 2. Perform a single grouped query to get Average and Count for all affected services at once
            var ratings = await Reviews
                .Where(r => serviceIds.Contains(r.ServiceId))
                .GroupBy(r => r.ServiceId)
                .Select(g => new
                {
                    ServiceId = g.Key,
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToListAsync();

            // 3. Update the corresponding Service entities
            foreach (var serviceId in serviceIds)
            {
                var service = await Services.FindAsync(serviceId);
                var ratingData = ratings.FirstOrDefault(r => r.ServiceId == serviceId);

                if (service != null)
                {
                    service.Rating = ratingData?.Average ?? 0;
                    service.ReviewCount = ratingData?.Count ?? 0;
                }
            }

            // 4. Save the new denormalized values
            await base.SaveChangesAsync();
        }
    }
}