using Microsoft.AspNetCore.Mvc;
using PortfolioApi.Services;
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

        await _emailService.SendContactEmailAsync(request);
        return Ok(new { success = true, message = "Message sent successfully! I'll get back to you within 24 hours." });
    }
}