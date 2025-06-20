using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeerTutoringSystem.Application.DTOs.Reviews;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PeerTutoringSystem.Api.Controllers.Reviews
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize(Roles = "Student,Tutor")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            try
            {
                var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ValidationException("Invalid token."));
                if (currentUserId != dto.StudentID)
                    return StatusCode(403, new { error = "You can only create a review as the student who booked the session." });

                var reviewId = await _reviewService.CreateReviewAsync(dto);
                return Ok(new { ReviewID = reviewId, message = "Review created successfully." });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("{reviewId:int}")]
        public async Task<IActionResult> GetReview(int reviewId)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(reviewId);
                return Ok(review);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("tutor/{tutorId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewsByTutor(Guid tutorId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByTutorIdAsync(tutorId);
                return Ok(reviews);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("tutor/{tutorId:guid}/average-rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAverageRatingByTutor(Guid tutorId)
        {
            try
            {
                var averageRating = await _reviewService.GetAverageRatingByTutorIdAsync(tutorId);
                return Ok(new { TutorId = tutorId, AverageRating = averageRating });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("top-tutors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopTutorsByRating([FromQuery] int count = 10)
        {
            try
            {
                var topTutors = await _reviewService.GetTopTutorsByRatingAsync(count);
                return Ok(topTutors);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred: " + ex.Message });
            }
        }
    }
}
