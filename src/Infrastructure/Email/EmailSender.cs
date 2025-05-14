using System.Net;
using System.Net.Mail;
using Application.Common.Interfaces.Utils;

namespace Infrastructure.Email;

public class EmailSender : IEmailSender
{
    private readonly string _email;
    private readonly string _password;

    public EmailSender(IAppSettings appSettings)
    {
        _email = appSettings.GmailEmail;
        _password = appSettings.GmailPassword;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        SmtpClient client = GetSmtpClient();

        MailMessage msg = new MailMessage
        {
            From = new MailAddress(_email),
            Subject = subject,
            To = { email },
            Body = htmlMessage,
            IsBodyHtml = true
        };
        return client.SendMailAsync(msg);
    }

    private SmtpClient GetSmtpClient()
    {
        return new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(_email, _password),
            EnableSsl = true
        };
    }
}