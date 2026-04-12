namespace Bookify.Application.DTO.Identity
{
    public class UpdateAdminProfileRequest
    {
        public string FullName { get; set; } = default!;

        public string Phone { get; set; } = default!;
    }
}
