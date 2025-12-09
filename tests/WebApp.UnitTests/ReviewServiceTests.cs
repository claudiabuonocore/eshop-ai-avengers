using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using eShop.Shared.Reviews;
using eShop.WebApp.Services;
using FluentAssertions;
using Xunit;

namespace WebApp.UnitTests;

public class ReviewServiceTests
{
    private static HttpClient CreateClient(HttpResponseMessage response, Func<HttpRequestMessage, bool>? assertReq = null)
    {
        var handler = new StubHandler(msg =>
        {
            assertReq?.Invoke(msg);
            return response;
        });
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
    }

    private class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    [Fact]
    public async Task SubmitAsync_Returns_Item_On_201()
    {
        var item = new ReviewItemDto
        {
            Id = Guid.NewGuid(),
            ProductId = 1,
            UserId = "u1",
            Rating = 5,
            Title = "t",
            Content = "c",
            CreatedAt = DateTime.UtcNow
        };
        var resp = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(item)
        };
        var client = CreateClient(resp, req => req.RequestUri!.ToString().EndsWith("api/reviews"));
        var svc = new ReviewService(client);

        var result = await svc.SubmitAsync(new SubmitReviewRequest { ProductId = 1, Rating = 5, Title = "t", Content = "c" });
        result.Should().NotBeNull();
        result!.ProductId.Should().Be(1);
    }

    [Fact]
    public async Task GetByProductAsync_Returns_PagedResult()
    {
        var page = new PagedResult<ReviewItemDto>
        {
            Items = new[]
            {
                new ReviewItemDto
                {
                    Id = Guid.NewGuid(), ProductId = 2, UserId = "u2", Rating = 4, Title = "t2", Content = "c2", CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        var resp = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(page)
        };
        var client = CreateClient(resp, req => req.RequestUri!.ToString().Contains("api/reviews/2"));
        var svc = new ReviewService(client);

        var result = await svc.GetByProductAsync(2);
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAggregateAsync_Returns_Data()
    {
        var agg = new ReviewAggregateDto { ReviewCount = 3, AverageRating = 4.5 };
        var resp = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(agg)
        };
        var client = CreateClient(resp, req => req.RequestUri!.ToString().EndsWith("/aggregate"));
        var svc = new ReviewService(client);

        var result = await svc.GetAggregateAsync(3);
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(4.5);
    }
}