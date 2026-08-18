using MailKit.Net.Smtp;
using System;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PortfolioApp.Api.Model;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace PortfolioApi.Services;


public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService>? _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config.GetSection("EmailSettings");
        _logger = logger;
    }

    public async Task SendContactEmailAsync(ContactDto contact)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_config["SenderName"], _config["SenderEmail"]));
        email.To.Add(new MailboxAddress("Your Name", _config["SenderEmail"])); // Sends to YOU
        email.Subject = $"Portfolio Contact: {contact.Subject}";

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $@"
            <h2>New Client Inquiry</h2>
            <p><strong>Name:</strong> {contact.Name}</p>
            <p><strong>Email:</strong> {contact.Email}</p>
            <p><strong>Message:</strong> {contact.Message}</p>
            <p><strong>Phone:</strong> {contact.Phone}</p >";
        email.Body = bodyBuilder.ToMessageBody();

        var enableProtocolLogging = bool.TryParse(_config["EnableProtocolLogging"], out var protLog) && protLog;
        var protocolLogPath = _config["ProtocolLogPath"] ?? "mailkit.log";
        ProtocolLogger? protocolLogger = null;
        if (enableProtocolLogging)
        {
            try
            {
                protocolLogger = new ProtocolLogger(protocolLogPath);
                _logger?.LogInformation("MailKit protocol logging enabled, writing to {Path}", protocolLogPath);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to create MailKit ProtocolLogger at {Path}", protocolLogPath);
                protocolLogger = null;
            }
        }

        using var smtp = protocolLogger != null ? new SmtpClient(protocolLogger) : new SmtpClient();

        // Optional: allow invalid certs (DEV ONLY). Configure "AllowInvalidCertificate": "true" under EmailSettings for dev.
        if (bool.TryParse(_config["AllowInvalidCertificate"], out var allowInvalid) && allowInvalid)
        {
            smtp.ServerCertificateValidationCallback = (object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
            {
                // Verbose logging for certificate validation in development. Do NOT accept untrusted certs in production.
                try
                {
                    _logger?.LogWarning("SMTP Server certificate validation invoked. SslPolicyErrors={Errors}", sslPolicyErrors);
                    if (chain != null)
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            _logger?.LogWarning("Certificate chain status: {Status} - {Info}", status.Status, status.StatusInformation);
                        }
                    }
                }
                catch { }

                // Accept everything in dev; do NOT use in production.
                return true;
            };
        }

        var host = _config["SmtpServer"];
        var port = int.TryParse(_config["Port"], out var p) ? p : 0;
        var secureOption = SecureSocketOptions.Auto;
        if (port == 465) secureOption = SecureSocketOptions.SslOnConnect;
        else if (port == 587) secureOption = SecureSocketOptions.StartTls;

        // Try primary connect, and for common Gmail case try fallback to 465 if 587 fails
        Exception? lastEx = null;
        var triedFallback = false;

        async Task<bool> TryConnectAndSendAsync(string h, int prt, SecureSocketOptions opt)
        {
            try
            {
                await smtp.ConnectAsync(h, prt, opt);
                await smtp.AuthenticateAsync(_config["Username"], _config["Password"]);
                await smtp.SendAsync(email);
                return true;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                try
                {
                    if (smtp.IsConnected)
                        await smtp.DisconnectAsync(true);
                }
                catch { }

                return false;
            }
        }

        try
        {
            var ok = await TryConnectAndSendAsync(host, port, secureOption);

            // If initial was STARTTLS on 587 and failed, try 465 with SSL (common Gmail requirement in some networks)
            if (!ok && port == 587 && secureOption == SecureSocketOptions.StartTls)
            {
                triedFallback = true;
                ok = await TryConnectAndSendAsync(host, 465, SecureSocketOptions.SslOnConnect);
            }

            if (!ok)
            {
                var hint = string.Empty;
                if (host?.Contains("gmail", StringComparison.OrdinalIgnoreCase) == true)
                {
                    hint = " Ensure you are using an App Password (if 2FA enabled) or OAuth2; plain account passwords are often blocked by Google.";
                }

                if (lastEx != null)
                    throw new InvalidOperationException($"Failed to send email via SMTP '{host}:{port}'{(triedFallback ? " (fallback to 465 attempted)" : string.Empty)}. {hint} See inner exception for details.", lastEx);

                throw new InvalidOperationException($"Failed to send email via SMTP '{host}:{port}'. {hint}");
            }
        }
        finally
        {
            if (smtp.IsConnected)
            {
                await smtp.DisconnectAsync(true);
            }

            try
            {
                protocolLogger?.Dispose();
            }
            catch { }
        }
    }
}