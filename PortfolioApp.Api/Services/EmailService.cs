using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PortfolioApp.Api.Model;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendContactEmailAsync(ContactDto contact)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var senderEmail = _config["EmailSettings:SenderEmail"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Portfolio", senderEmail));
        message.To.Add(new MailboxAddress("Admin", senderEmail)); // or another email
        message.Subject = $"New Contact: {contact.Name}";
        message.Body = new TextPart("plain")
        {
            Text = $"Name: {contact.Name}\nEmail: {contact.Email}\nMessage: {contact.Message}"
        };

        using var client = new SmtpClient();

        // Connect with SSL disabled (STARTTLS will be used)
        await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

        // Authenticate with App Password
        await client.AuthenticateAsync(username, password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}