using Microsoft.EntityFrameworkCore;
using PortfolioApp.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep C# property names
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Email Service
builder.Services.AddTransient<EmailService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins("https://your-app.vercel.app")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Auto-create database and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Creates DB if not exists

    if (!db.Projects.Any())
    {
        db.Projects.AddRange(
            new PortfolioApp.Api.Model.ProjectEntity { Title = "E-Commerce Platform", Description = "Full-stack e-commerce with payment integration.", Image = "https://picsum.photos/seed/ecommerce/600/400.jpg", Tags = "Angular,.NET 8,Stripe", LiveUrl = "#", GithubUrl = "#", Featured = true },
            new PortfolioApp.Api.Model.ProjectEntity { Title = "Real-Time Chat", Description = "Scalable chat application with SignalR.", Image = "https://picsum.photos/seed/chatapp/600/400.jpg", Tags = "Angular,SignalR,Redis", GithubUrl = "#", Featured = true },
            new PortfolioApp.Api.Model.ProjectEntity { Title = "Portfolio CMS", Description = "Headless CMS for developer portfolios.", Image = "https://picsum.photos/seed/cms/600/400.jpg", Tags = ".NET 8,MongoDB,Angular", GithubUrl = "#", Featured = false }
        );
        db.SaveChanges();
    }
}

app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();