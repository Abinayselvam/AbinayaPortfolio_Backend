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
        if (request == null || !ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid form payload." });
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _emailService.SendContactEmailAsync(request, cts.Token);
            return Ok(new { success = true, message = "Message sent successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contact form email sending failed.");

            // Returns the EXACT exception message directly to your browser for immediate debugging
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message,
                innerError = ex.InnerException?.Message,
                type = ex.GetType().Name
            });
        }
    }
}