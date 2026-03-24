namespace Bookify.Application.Interfaces.Email
{
    /// <summary>
    /// Service for sending emails.
    /// </summary>
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
