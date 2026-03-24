using Bookify.Application.DTO.Review;
using Bookify.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : BaseController
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>Submit a review for a completed booking (Client only).</summary>
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var result = await _reviewService.CreateReviewAsync(request);
            return HandleResult(result);
        }

        /// <summary>Get paginated reviews for a service (public).</summary>
        [HttpGet("service/{serviceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceReviews(
            Guid serviceId,
            [FromQuery] int page     = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetReviewsByServiceAsync(serviceId, page, pageSize);
            return HandleResult(result);
        }

        /// <summary>Get paginated reviews submitted by the authenticated client.</summary>
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetMyReviews(
            [FromQuery] int page     = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetReviewsByClientAsync(CurrentUserGuid, page, pageSize);
            return HandleResult(result);
        }
    }
}
