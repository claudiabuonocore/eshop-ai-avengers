using System.Net.Http.Json;
using eShop.Shared.Reviews;

namespace eShop.WebApp.Services;

public interface IReviewService
{
    Task<ReviewItemDto?> SubmitAsync(SubmitReviewRequest request, CancellationToken ct = default);
    Task<PagedResult<ReviewItemDto>> GetByProductAsync(int productId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ReviewAggregateDto?> GetAggregateAsync(int productId, CancellationToken ct = default);
}

public class ReviewService(HttpClient httpClient) : IReviewService
{
    public async Task<ReviewItemDto?> SubmitAsync(SubmitReviewRequest request, CancellationToken ct = default)
    {
        var resp = await httpClient.PostAsJsonAsync("api/reviews", request, ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ReviewItemDto>(cancellationToken: ct);
    }

    public async Task<PagedResult<ReviewItemDto>> GetByProductAsync(int productId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<PagedResult<ReviewItemDto>>($"api/reviews/{productId}?page={page}&pageSize={pageSize}", ct);
        return result ?? new PagedResult<ReviewItemDto>
        {
            Items = Array.Empty<ReviewItemDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewAggregateDto?> GetAggregateAsync(int productId, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<ReviewAggregateDto>($"api/reviews/{productId}/aggregate", ct);
    }
}