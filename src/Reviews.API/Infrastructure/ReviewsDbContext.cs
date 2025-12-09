using eShop.Reviews.API.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Reviews.API.Infrastructure
{
    public class ReviewsDbContext : DbContext
    {
        public ReviewsDbContext(DbContextOptions<ReviewsDbContext> options) : base(options)
        {
        }

        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var review = modelBuilder.Entity<ProductReview>();
            review.HasKey(r => r.Id);
            review.Property(r => r.ProductId).IsRequired();
            review.Property(r => r.UserId).IsRequired();
            review.Property(r => r.Rating).IsRequired();
            review.Property(r => r.Title).IsRequired().HasMaxLength(200);
            review.Property(r => r.Content).IsRequired().HasMaxLength(2000);
            review.Property(r => r.CreatedAt).IsRequired();

            review.HasIndex(r => r.ProductId).HasDatabaseName("IX_ProductReviews_ProductId");
            review.HasIndex(r => r.CreatedAt).HasDatabaseName("IX_ProductReviews_CreatedAt");

            review.ToTable("ProductReviews");
        }
    }
}
