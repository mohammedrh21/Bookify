using System.ComponentModel.DataAnnotations;

namespace Bookify.Client.Models.Profile
{
    public class AdminProfileModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Role { get; set; } = "Admin";
    }

    public class UpdateAdminProfileModel
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;
    }
}
