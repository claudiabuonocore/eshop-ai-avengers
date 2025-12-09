using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShop.Reviews.API.Infrastructure;
using eShop.Reviews.API.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Reviews.API.Application
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewsDbContext _db;

        public ReviewRepository(ReviewsDbContext db)
        {
            _db = db;
        }

        public async Task<ProductReview> AddAsync(ProductReview review)
        {
            Validate(review);
            review.Id = review.Id == Guid.Empty ? Guid.NewGuid() : review.Id;
            review.CreatedAt = review.CreatedAt == default ? DateTime.UtcNow : review.CreatedAt;

            _db.ProductReviews.Add(review);
            await _db.SaveChangesAsync();
            return review;
        }

        public async Task<(IReadOnlyList<ProductReview> Items, int TotalCount)> GetByProductAsync(int productId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(int productId)
        {
            var query = _db.ProductReviews.AsNoTracking().Where(r => r.ProductId == productId);
            var count = await query.CountAsync();
            if (count == 0) return (0, 0);
            var avg = await query.AverageAsync(r => (double)r.Rating);
            return (avg, count);
        }

        private static void Validate(ProductReview review)
        {
            if (review.Rating < 1 || review.Rating > 5)
                throw new ArgumentOutOfRangeException(nameof(review.Rating), "Rating must be between 1 and 5.");
            if (string.IsNullOrWhiteSpace(review.Title) || review.Title.Length < 10 || review.Title.Length > 200)
                throw new ArgumentException("Title must be 10-200 characters.", nameof(review.Title));
            if (string.IsNullOrWhiteSpace(review.Content) || review.Content.Length < 50 || review.Content.Length > 2000)
                throw new ArgumentException("Content must be 50-2000 characters.", nameof(review.Content));
            if (string.IsNullOrWhiteSpace(review.UserId))
                throw new ArgumentException("UserId is required.", nameof(review.UserId));
        }
    }
}
