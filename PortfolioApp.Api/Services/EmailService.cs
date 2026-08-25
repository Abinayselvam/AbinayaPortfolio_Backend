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

    public async Task SendContactEmailAsync(ContactDto contact, CancellationToken cancellationToken = default)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = _config.GetValue<int?>("EmailSettings:SmtpPort") ?? 465;
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? username;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Email credentials missing. Please configure Username and Password in Render Environment.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Portfolio Contact", senderEmail));
        message.To.Add(new MailboxAddress("Admin", senderEmail));
        message.Subject = $"New Contact: {contact.Name}";

        message.Body = new TextPart("plain")
        {
            Text = $"Name: {contact.Name}\nEmail: {contact.Email}\nMessage: {contact.Message}"
        };

        using var client = new SmtpClient();

        // Bypass SSL certificate check issues on cloud environments
        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        // Use SslOnConnect for Port 465, or StartTls for Port 587
        var socketOption = (smtpPort == 465)
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(smtpServer, smtpPort, socketOption, cancellationToken);
        await client.AuthenticateAsync(username, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}