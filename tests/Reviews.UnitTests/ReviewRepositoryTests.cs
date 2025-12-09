using System;
using System.Linq;
using System.Threading.Tasks;
using eShop.Reviews.API.Application;
using eShop.Reviews.API.Infrastructure;
using eShop.Reviews.API.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Reviews.UnitTests
{
    public class ReviewRepositoryTests
    {
        private ReviewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReviewsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ReviewsDbContext(options);
        }

        private static ProductReview ValidReview(int productId = 1, string? userId = "user-1") => new ProductReview
        {
            ProductId = productId,
            UserId = userId!,
            Rating = 5,
            Title = new string('T', 10),
            Content = new string('C', 50)
        };

        [Fact]
        public async Task AddAsync_Sets_Id_And_CreatedAt()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            var review = ValidReview();

            var saved = await repo.AddAsync(review);

            saved.Id.Should().NotBe(Guid.Empty);
            saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task GetByProductAsync_Paginates_And_Orders_Newest_First()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            for (int i = 0; i < 30; i++)
            {
                await repo.AddAsync(new ProductReview
                {
                    ProductId = 1,
                    UserId = "u",
                    Rating = 3,
                    Title = new string('A', 10),
                    Content = new string('B', 50),
                    CreatedAt = DateTime.UtcNow.AddMinutes(i)
                });
            }

            var (items, total) = await repo.GetByProductAsync(1, page: 2, pageSize: 10);

            total.Should().Be(30);
            items.Should().HaveCount(10);
            items.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
            items.First().CreatedAt.Should().BeAfter(items.Last().CreatedAt);
        }

        [Fact]
        public async Task GetAggregateAsync_Returns_Average_And_Count()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            await repo.AddAsync(ValidReview(productId: 1));
            await repo.AddAsync(new ProductReview
            {
                ProductId = 1,
                UserId = "u",
                Rating = 1,
                Title = new string('A', 10),
                Content = new string('B', 50)
            });

            var (avg, count) = await repo.GetAggregateAsync(1);
            avg.Should().BeApproximately(3.0, 0.001);
            count.Should().Be(2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task AddAsync_Rejects_Invalid_Rating(int rating)
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            var review = ValidReview();
            review.Rating = rating;

            var act = async () => await repo.AddAsync(review);
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task AddAsync_Rejects_Short_Title()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            var review = ValidReview();
            review.Title = "short";

            var act = async () => await repo.AddAsync(review);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddAsync_Rejects_Short_Content()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            var review = ValidReview();
            review.Content = "tiny";

            var act = async () => await repo.AddAsync(review);
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddAsync_Rejects_Missing_UserId()
        {
            using var ctx = CreateContext();
            var repo = new ReviewRepository(ctx);
            var review = ValidReview(userId: "");

            var act = async () => await repo.AddAsync(review);
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
