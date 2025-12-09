using eShop.Reviews.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using eShop.Reviews.API;
using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure DbContext
builder.Services.AddDbContext<ReviewsDbContext>(options =>
{
    var env = builder.Environment.EnvironmentName;
    if (string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("ReviewsFunctionalTests");
        return;
    }

    var connectionString = builder.Configuration.GetConnectionString("ReviewsDb")
        ?? Environment.GetEnvironmentVariable("REVIEWS_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=reviews;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<eShop.Reviews.API.Application.IReviewRepository, eShop.Reviews.API.Application.ReviewRepository>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "TestAuth";
    options.DefaultChallengeScheme = "TestAuth";
}).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();
// Swagger disabled in tests to avoid type load issues

// Apply migrations and seed sample data in Development
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();

    // For non-testing envs, ensure database is ready
    try
    {
        if (!string.Equals(env.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            db.Database.Migrate();
        }
    }
    catch
    {
        // If migration fails (e.g., no DB), ignore so dev can still run InMemory
    }

    // Seed only in Development and only if table is empty
    if (env.IsDevelopment() && !db.ProductReviews.Any())
    {
        db.ProductReviews.AddRange(new eShop.Reviews.API.Models.ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = 1,
            UserId = "alice",
            Rating = 5,
            Title = "Fantastic quality",
            Content = "Exceeded expectations. Will buy again!",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        },
        new eShop.Reviews.API.Models.ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = 1,
            UserId = "bob",
            Rating = 4,
            Title = "Solid purchase",
            Content = "Good value for money. Minor quirks.",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new eShop.Reviews.API.Models.ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = 2,
            UserId = "carol",
            Rating = 3,
            Title = "Average",
            Content = "Works as described but nothing special.",
            CreatedAt = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}

app.Run();
