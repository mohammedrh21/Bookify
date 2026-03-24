using System.ComponentModel.DataAnnotations;

namespace Bookify.Client.Models.Profile
{
    public class ClientProfileModel
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

    public class UpdateClientProfileModel
    {
        [Required, MinLength(2)]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;

        public GenderType? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
