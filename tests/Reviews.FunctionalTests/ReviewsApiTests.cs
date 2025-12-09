using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using eShop.Shared.Reviews;
using Xunit;

namespace Reviews.FunctionalTests
{
    public class ReviewsApiTests
    {
        private readonly WebApplicationFactory<Program> _factory = new();

        [Fact]
        public async Task Post_Submit_Returns_Created_And_Persists()
        {
            var client = _factory.CreateClient();

            var request = new SubmitReviewRequest
            {
                ProductId = 100,
                Rating = 5,
                Title = new string('T', 10),
                Content = new string('C', 50)
            };

            var response = await client.PostAsJsonAsync("/api/reviews", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<ReviewItemDto>();
            created!.ProductId.Should().Be(100);

            var list = await client.GetFromJsonAsync<PagedResult<ReviewItemDto>>($"/api/reviews/{request.ProductId}?page=1&pageSize=10");
            list!.TotalCount.Should().BeGreaterOrEqualTo(1);
            list.Items.Should().Contain(x => x.Id == created.Id);
        }

        [Fact]
        public async Task Get_Aggregate_Returns_ExpectedValues()
        {
            var client = _factory.CreateClient();
            var pid = 200;
            await client.PostAsJsonAsync("/api/reviews", new SubmitReviewRequest { ProductId = pid, Rating = 4, Title = new string('A', 10), Content = new string('B', 50) });
            await client.PostAsJsonAsync("/api/reviews", new SubmitReviewRequest { ProductId = pid, Rating = 2, Title = new string('A', 10), Content = new string('B', 50) });

            var agg = await client.GetFromJsonAsync<ReviewAggregateDto>($"/api/reviews/{pid}/aggregate");
            agg!.ReviewCount.Should().Be(2);
            agg.AverageRating.Should().BeApproximately(3.0, 0.001);
        }
    }
}
