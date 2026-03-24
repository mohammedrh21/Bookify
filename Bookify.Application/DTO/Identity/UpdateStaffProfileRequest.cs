using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Identity
{
    public class UpdateStaffProfileRequest
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }
    }
}
