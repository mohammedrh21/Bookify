namespace Bookify.Application.DTO.SupportTicket
{
    public class CreateTicketRequest
    {
        public string Email { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsRead { get; set; }
    }
}
