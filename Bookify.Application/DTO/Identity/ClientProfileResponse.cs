using Bookify.Domain.Enums;

namespace Bookify.Application.DTO.Identity
{
    public class ClientProfileResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Role { get; set; } = "Client";
        public GenderType? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImagePath { get; set; }
    }
}
