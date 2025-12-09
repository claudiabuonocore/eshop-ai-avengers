using System;
using eShop.Reviews.API.Infrastructure;
using eShop.Reviews.API.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Reviews.UnitTests
{
    public class ReviewsDbContextTests
    {
        private ReviewsDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ReviewsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ReviewsDbContext(options);
        }

        [Fact]
        public void Model_Configures_Required_Fields_And_Lengths()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(ProductReview));
            entity.Should().NotBeNull();

            entity!.FindProperty(nameof(ProductReview.ProductId))!.IsNullable.Should().BeFalse();
            entity!.FindProperty(nameof(ProductReview.UserId))!.IsNullable.Should().BeFalse();
            entity!.FindProperty(nameof(ProductReview.Rating))!.IsNullable.Should().BeFalse();
            entity!.FindProperty(nameof(ProductReview.Title))!.IsNullable.Should().BeFalse();
            entity!.FindProperty(nameof(ProductReview.Content))!.IsNullable.Should().BeFalse();
            entity!.FindProperty(nameof(ProductReview.CreatedAt))!.IsNullable.Should().BeFalse();

            entity!.FindProperty(nameof(ProductReview.Title))!.GetMaxLength().Should().Be(200);
            entity!.FindProperty(nameof(ProductReview.Content))!.GetMaxLength().Should().Be(2000);
        }

        [Fact]
        public void Model_Has_Indexes_On_ProductId_And_CreatedAt()
        {
            using var ctx = CreateContext();
            var entity = ctx.Model.FindEntityType(typeof(ProductReview))!;
            var productIdIndex = entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ProductReview.ProductId)));
            var createdAtIndex = entity.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ProductReview.CreatedAt)));

            productIdIndex.Should().NotBeNull();
            createdAtIndex.Should().NotBeNull();
        }
    }
}
