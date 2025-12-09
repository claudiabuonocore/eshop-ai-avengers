using System;
using System.Collections.Generic;

namespace eShop.Shared.Reviews
{
    public class SubmitReviewRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ReviewItemDto
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewAggregateDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
