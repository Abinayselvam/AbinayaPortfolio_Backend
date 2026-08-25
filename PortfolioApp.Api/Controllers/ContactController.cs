using Microsoft.AspNetCore.Mvc;
using PortfolioApp.Api.Model;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(EmailService emailService, ILogger<ContactController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] ContactDto request)
    {
        _logger.LogInformation("Received contact form submission for {Email}", request?.Email);

        if (request == null || !ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid form data submitted." });
        }

        try
        {
            // Give SMTP a strict 8-second timeout window so the API request doesn't hang indefinitely
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

            await _emailService.SendContactEmailAsync(request);

            return Ok(new { success = true, message = "Message sent successfully!" });
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("SMTP timed out when trying to send email.");
            return StatusCode(500, new { success = false, message = "Email service timed out. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email.");
            return StatusCode(500, new { success = false, message = "Email failed to send.", details = ex.Message });
        }
    }
}