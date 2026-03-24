using System.ComponentModel.DataAnnotations;

namespace Bookify.Application.DTO.Identity
{
    public class UpdateAdminProfileRequest
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;
    }
}
