using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PortfolioApp.Api.Model;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendContactEmailAsync(ContactDto contact, CancellationToken cancellationToken = default)
    {
        // 1. Fetch values safely with defaults
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var portString = _config["EmailSettings:SmtpPort"] ?? "587";
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? username;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogError("SMTP credentials (Username/Password) are missing in configuration.");
            throw new InvalidOperationException("Email service is missing configuration credentials.");
        }

        int.TryParse(portString, out int smtpPort);
        smtpPort = smtpPort == 0 ? 587 : smtpPort;

        // 2. Build MimeMessage
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Portfolio Contact", senderEmail));
        message.To.Add(new MailboxAddress("Admin", senderEmail));
        message.Subject = $"New Contact Form Submission: {contact.Name}";

        message.Body = new TextPart("plain")
        {
            Text = $"Name: {contact.Name}\nEmail: {contact.Email}\nMessage: {contact.Message}"
        };

        // 3. Connect & Send with MailKit
        using var client = new SmtpClient();

        try
        {
            // Auto-detect SSL/TLS options based on standard ports (587 vs 465)
            var socketOptions = smtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(smtpServer, smtpPort, socketOptions, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}