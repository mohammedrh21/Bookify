namespace Bookify.Client.Models.FAQ;

public class FaqModel
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
