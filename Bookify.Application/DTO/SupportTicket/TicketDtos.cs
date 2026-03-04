namespace Bookify.Application.DTO.SupportTicket
{
    public record CreateTicketRequest(string Email, string Subject, string Description);

    public record TicketResponse(
        Guid Id,
        string Email,
        string Subject,
        string Description,
        DateTime CreatedAt);
}
