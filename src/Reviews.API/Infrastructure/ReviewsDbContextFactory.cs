using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace eShop.Reviews.API.Infrastructure
{
    public class ReviewsDbContextFactory : IDesignTimeDbContextFactory<ReviewsDbContext>
    {
        public ReviewsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ReviewsDbContext>();
            var connStr = Environment.GetEnvironmentVariable("REVIEWS_DB_CONNECTION")
                          ?? "Host=localhost;Port=5432;Database=eshop_reviews;Username=postgres;Password=postgres";
            optionsBuilder.UseNpgsql(connStr);
            return new ReviewsDbContext(optionsBuilder.Options);
        }
    }
}
