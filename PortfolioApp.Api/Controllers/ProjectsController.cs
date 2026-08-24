using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Api.Data;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Projects
    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _context.Projects
            .Select(p => new {
                p.Id,
                p.Title,
                p.Description,
                p.Image,
                Tags = p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries),
                p.LiveUrl,
                p.GithubUrl,
                p.Featured
            }).ToListAsync();

        return Ok(projects);
    }
}