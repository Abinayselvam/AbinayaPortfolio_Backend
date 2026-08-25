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

// SAFELY CONFIGURE DATABASE (Use In-Memory on Render Linux to prevent LocalDB crashes)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    // Production / Render environment fallback
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("PortfolioDb"));
}

// Email Service
builder.Services.AddTransient<EmailService>();

// Dynamic CORS Policy for Vercel & Localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercelFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
                string.IsNullOrEmpty(origin) ||
                new Uri(origin).Host.EndsWith("vercel.app") ||
                origin.StartsWith("http://localhost"))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS Middleware (Must be before MapControllers)
app.UseCors("AllowVercelFrontend");

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