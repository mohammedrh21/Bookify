using Bookify.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Identity
{
    public class UpdateClientProfileRequest
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
