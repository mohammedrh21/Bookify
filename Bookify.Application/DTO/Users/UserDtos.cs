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
}
