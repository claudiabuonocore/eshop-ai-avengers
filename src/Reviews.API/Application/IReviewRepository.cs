using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.Reviews.API.Models;

namespace eShop.Reviews.API.Application
{
    public interface IReviewRepository
    {
        Task<ProductReview> AddAsync(ProductReview review);

        Task<(IReadOnlyList<ProductReview> Items, int TotalCount)> GetByProductAsync(
            int productId,
            int page,
            int pageSize);

        Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(int productId);
    }
}
