using Microsoft.EntityFrameworkCore;
using PortfolioApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Email Service
builder.Services.AddTransient<EmailService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy
            .WithOrigins("https://your-app.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();


// ============================================================
// DATABASE MIGRATION + SEED DATA
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Apply pending EF Core migrations
        //context.Database.Migrate();

        // Seed initial project data
        if (!context.Projects.Any())
        {
            context.Projects.AddRange(
                new PortfolioApp.Api.Model.ProjectEntity
                {
                    Title = "E-Commerce Platform",
                    Description = "Full-stack e-commerce with payment integration.",
                    Image = "https://picsum.photos/seed/ecommerce/600/400.jpg",
                    Tags = "Angular,.NET 8,Stripe",
                    LiveUrl = "#",
                    GithubUrl = "#",
                    Featured = true
                },

                new PortfolioApp.Api.Model.ProjectEntity
                {
                    Title = "Real-Time Chat",
                    Description = "Scalable chat application with SignalR.",
                    Image = "https://picsum.photos/seed/chatapp/600/400.jpg",
                    Tags = "Angular,SignalR,Redis",
                    GithubUrl = "#",
                    Featured = true
                },

                new PortfolioApp.Api.Model.ProjectEntity
                {
                    Title = "Portfolio CMS",
                    Description = "Headless CMS for developer portfolios.",
                    Image = "https://picsum.photos/seed/cms/600/400.jpg",
                    Tags = ".NET 8,MongoDB,Angular",
                    GithubUrl = "#",
                    Featured = false
                }
            );

            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            ex,
            "Database migration or seed operation failed."
        );

        // Do NOT stop the application.
        // The API can still start, and the error will be visible in logs.
    }
}


// CORS
app.UseCors("AllowAngularApp");


// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();