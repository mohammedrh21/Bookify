using Bookify.Domain.Enums;

namespace Bookify.Application.DTO.Identity
{
    public class UpdateClientProfileRequest
    {
        public string FullName { get; set; } = default!;

        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
