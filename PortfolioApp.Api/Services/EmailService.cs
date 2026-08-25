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
        // 1. Read options safely with default fallback values
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = _config.GetValue<int?>("EmailSettings:SmtpPort") ?? 587;
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? username;

        // 2. Fail with a clear message if credentials are empty
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Email service credentials (Username or Password) are missing from configuration.");
        }

        // 3. Construct MimeMessage
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Portfolio Contact", senderEmail));
        message.To.Add(new MailboxAddress("Admin", senderEmail));
        message.Subject = $"New Contact Form Submission: {contact.Name}";

        message.Body = new TextPart("plain")
        {
            Text = $"Name: {contact.Name}\nEmail: {contact.Email}\nMessage: {contact.Message}"
        };

        // 4. Connect with MailKit
        using var client = new SmtpClient();

        var socketOptions = smtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(smtpServer, smtpPort, socketOptions, cancellationToken);
        await client.AuthenticateAsync(username, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}