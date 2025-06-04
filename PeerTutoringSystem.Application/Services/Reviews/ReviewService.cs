using PeerTutoringSystem.Application.DTOs.Reviews;
using PeerTutoringSystem.Application.Interfaces.Reviews;
using PeerTutoringSystem.Domain.Entities.Booking;
using PeerTutoringSystem.Domain.Entities.Reviews;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Reviews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Application.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingSessionRepository _bookingRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IBookingSessionRepository bookingRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
        }

        public async Task<int> CreateReviewAsync(CreateReviewDto dto)
        {
            ValidateDto(dto);

            // Validate Booking
            var booking = await _bookingRepository.GetByIdAsync(dto.BookingID);
            if (booking == null)
                throw new ValidationException("Booking not found.");

            // Ensure the booking is completed
            if (booking.Status != BookingStatus.Completed)
                throw new ValidationException("Cannot review a session that is not completed.");

            // Validate Student and Tutor
            var student = await _userRepository.GetByIdAsync(dto.StudentID);
            var tutor = await _userRepository.GetByIdAsync(dto.TutorID);
            if (student == null || tutor == null)
                throw new ValidationException("Student or Tutor not found.");

            // Ensure the student is the one who booked the session
            if (booking.StudentId != dto.StudentID)
                throw new ValidationException("Only the student who booked the session can leave a review.");

            // Ensure the tutor is the one assigned to the session
            if (booking.TutorId != dto.TutorID)
                throw new ValidationException("The tutor ID does not match the session's tutor.");

            // Check if a review already exists for this booking
            var existingReview = await _reviewRepository.GetByBookingIdAsync(dto.BookingID);
            if (existingReview != null)
                throw new ValidationException("A review for this booking already exists.");

            var review = new Review
            {
                BookingID = dto.BookingID,
                StudentID = dto.StudentID,
                TutorID = dto.TutorID,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ReviewDate = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            return review.ReviewID;
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new ValidationException("Review not found.");

            return new ReviewDto
            {
                ReviewID = review.ReviewID,
                BookingID = review.BookingID,
                StudentID = review.StudentID,
                TutorID = review.TutorID,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByTutorIdAsync(Guid tutorId)
        {
            var tutor = await _userRepository.GetByIdAsync(tutorId);
            if (tutor == null || tutor.Role.RoleName != "Tutor")
                throw new ValidationException("Tutor not found.");

            var reviews = await _reviewRepository.GetByTutorIdAsync(tutorId);
            return reviews.Select(review => new ReviewDto
            {
                ReviewID = review.ReviewID,
                BookingID = review.BookingID,
                StudentID = review.StudentID,
                TutorID = review.TutorID,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate
            }).ToList();
        }

        private void ValidateDto<T>(T dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }

            // Additional validation for Rating
            var createReviewDto = dto as CreateReviewDto;
            if (createReviewDto != null && (createReviewDto.Rating < 1 || createReviewDto.Rating > 5))
                throw new ValidationException("Rating must be between 1 and 5.");
        }
    }
}
