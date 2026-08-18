using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Api.Data;
using PortfolioApp.Api.Model;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/Analytics/visit
    [HttpPost("visit")]
    public async Task<IActionResult> LogVisit()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var log = new VisitorLog
        {
            IpAddress = ip,
            UserAgent = Request.Headers.UserAgent
        };
        _context.VisitorLogs.Add(log);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // GET: api/Analytics/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var total = await _context.VisitorLogs.CountAsync();
        var today = await _context.VisitorLogs.CountAsync(v => v.VisitedAt.Date == DateTime.UtcNow.Date);
        return Ok(new { total, today });
    }
}