using Bookify.Domain.Entities;
using Bookify.Infrastructure.Identity.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Bookify.Infrastructure.Data
{
    public class AppDbContext
    : IdentityDbContext<ApplicationIdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Domain.Entities.Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
        }
    }
}