using System;
using System.Linq;
using System.Threading.Tasks;
using eShop.Reviews.API.Application;
using eShop.Reviews.API.Models;
using eShop.Shared.Reviews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace eShop.Reviews.API.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _repo;

        public ReviewsController(IReviewRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Submit([FromBody] SubmitReviewRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Rating must be between 1 and 5.");
            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 10 || request.Title.Length > 200)
                return BadRequest("Title must be 10-200 characters.");
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 50 || request.Content.Length > 2000)
                return BadRequest("Content must be 50-2000 characters.");

            var userId = User.FindFirstValue("sub")
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.Identity?.Name
                         ?? "anonymous";

            var review = new ProductReview
            {
                ProductId = request.ProductId,
                UserId = userId,
                Rating = request.Rating,
                Title = request.Title,
                Content = request.Content
            };

            var saved = await _repo.AddAsync(review);

            var dto = new ReviewItemDto
            {
                Id = saved.Id,
                ProductId = saved.ProductId,
                UserId = saved.UserId,
                Rating = saved.Rating,
                Title = saved.Title,
                Content = saved.Content,
                CreatedAt = saved.CreatedAt
            };

            return Created($"/api/reviews/{dto.ProductId}/{dto.Id}", dto);
        }

        [HttpGet("{productId:int}")]
        public async Task<ActionResult<PagedResult<ReviewItemDto>>> GetByProduct([FromRoute] int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _repo.GetByProductAsync(productId, page, pageSize);
            var dtos = items.Select(r => new ReviewItemDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Rating = r.Rating,
                Title = r.Title,
                Content = r.Content,
                CreatedAt = r.CreatedAt
            }).ToList();

            var result = new PagedResult<ReviewItemDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
            return Ok(result);
        }

        [HttpGet("{productId:int}/aggregate")]
        public async Task<ActionResult<ReviewAggregateDto>> GetAggregate([FromRoute] int productId)
        {
            var (avg, count) = await _repo.GetAggregateAsync(productId);
            return Ok(new ReviewAggregateDto { AverageRating = avg, ReviewCount = count });
        }
    }
}
