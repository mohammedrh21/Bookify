using System.ComponentModel.DataAnnotations;

namespace Bookify.Client.Models.Profile
{
    public class StaffProfileModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Role { get; set; } = "Staff";
        public GenderType? Gender { get; set; }
        public string? ImagePath { get; set; }
    }

    public class UpdateStaffProfileModel
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        [StringLength(15, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 15 characters.")]
        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }
    }
}
