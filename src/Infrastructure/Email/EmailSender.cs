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

    public Task SendEmailWithDefaultDesignAsync(string email, string subject, string message)
    {
        SmtpClient client = GetSmtpClient();

        MailMessage msg = new MailMessage
        {
            From = new MailAddress(_email),
            Subject = subject,
            To = { email },
            Body = HtmlMessage(subject, message),
            IsBodyHtml = true
        };
        return client.SendMailAsync(msg);
    }

    private static string HtmlMessage(string subject, string message)
    {
        string htmlMessage = $@"
        <!DOCTYPE html>
        <html>
        <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                margin: 0;
                padding: 20px;
                background-color: #f4f4f4;
            }}
            .container {{
                background-color: #ffffff;
                padding: 20px;
                border-radius: 5px;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                color: #333333;
                border-bottom: 1px solid #eeeeee;
                padding-bottom: 10px;
                margin-bottom: 20px;
            }}
            .content {{
                color: #555555;
                line-height: 1.6;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 0.9em;
                color: #777777;
                text-align: center;
            }}
        </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Response to your report: {subject}</h2>
                </div>
                <div class='content'>
                    <p>Hello!</p>
                    <p>{message}</p>
                    <p>Thank you for reaching out.</p>
                </div>
                <div class='footer'>
                    <p>&copy; {DateTime.Now.Year} Currency Monitor</p>
                </div>
            </div>
        </body>
        </html>
        ";

        return htmlMessage;
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