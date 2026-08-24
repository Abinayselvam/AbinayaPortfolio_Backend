using Microsoft.EntityFrameworkCore;
using PortfolioApp.Api.Model;


namespace PortfolioApp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
        public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    }
}
