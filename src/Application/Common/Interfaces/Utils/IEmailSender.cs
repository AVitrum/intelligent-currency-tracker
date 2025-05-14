namespace Application.Common.Interfaces.Utils;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}