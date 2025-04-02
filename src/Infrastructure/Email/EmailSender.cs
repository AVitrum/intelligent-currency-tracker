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

    public Task SendEmailAsync(string email, string subject, string message)
    {
        SmtpClient client = GetSmtpClient();

        MailMessage msg = new MailMessage
        {
            From = new MailAddress(_email),
            Subject = subject,
            To = { email },
            Body = $"""
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #f4f4f4; padding: 20px;'>
                            <h2 style='color: #333;'>Hello, User!</h2>
                            <p style='color: #555;'>{message}</p>
                            <p style='color: #777;'>Best regards,<br>Company Name</p>
                        </div>
                    </body>
                    </html>
                    """,
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