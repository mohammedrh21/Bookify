using Bookify.Domain.Enums;

namespace Bookify.Application.DTO.Identity
{
    public class UpdateStaffProfileRequest
    {
        public string FullName { get; set; } = default!;

        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }
    }
}
