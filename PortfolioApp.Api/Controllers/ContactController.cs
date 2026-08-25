using Microsoft.AspNetCore.Mvc;
using PortfolioApp.Api.Model;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly EmailService _emailService;

    public ContactController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] ContactDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            // Set a strict 10-second cancellation token so SMTP doesn't hang forever
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await _emailService.SendContactEmailAsync(request);

            return Ok(new { success = true, message = "Message sent successfully!" });
        }
        catch (Exception ex)
        {
            // Return 500 error immediately if SMTP fails or times out
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to send email. Check SMTP settings.",
                error = ex.Message
            });
        }
    }
}