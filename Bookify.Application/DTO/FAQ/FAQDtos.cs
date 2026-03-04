namespace Bookify.Application.DTO.FAQ
{
    public record CreateFAQRequest(string Question, string Answer);

    public record UpdateFAQRequest(Guid Id, string Question, string Answer);

    public record FAQResponse(Guid Id, string Question, string Answer);
}
