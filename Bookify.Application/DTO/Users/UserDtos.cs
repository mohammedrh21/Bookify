using Bookify.Domain.Entities;

namespace Bookify.Application.DTO.Users
{
    public class StaffDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public string? ImagePath { get; set; }
    }

    public class ClientDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int BookingCount { get; set; }
        public string? ImagePath { get; set; }
    }

    public class ClientReportDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalBookings { get; set; }
        public string? ImagePath { get; set; }
        public List<ClientReportBookingDto> RecentBookings { get; set; } = new();
    }

    public class ClientReportBookingDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StaffClientDto
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? FirstBookingDate { get; set; }
        public DateTime? LastBookingDate { get; set; }
    }

    public class StaffClientDetailsDto
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<StaffClientBookingDto> Bookings { get; set; } = new();
    }

    public class StaffClientBookingDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class AdminClientDto
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int TotalBookings { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminClientDetailsDto
    {
        public Guid ClientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<AdminClientBookingDto> Bookings { get; set; } = new();
    }

    public class AdminClientBookingDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }

    // ── Admin Staff DTOs ───────────────────────────────────────────────────────

    public class AdminStaffDto
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public Guid? ServiceId { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public double Rating { get; set; }
        public DateTime? JoinedDate { get; set; }
    }

    public class AdminStaffDetailsDto
    {
        public Guid StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }

        public Guid? ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public List<AdminStaffBookingDto> Bookings { get; set; } = new();
    }

    public class AdminStaffBookingDto
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
