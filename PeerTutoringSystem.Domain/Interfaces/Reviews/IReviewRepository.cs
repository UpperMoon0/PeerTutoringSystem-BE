using PeerTutoringSystem.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Reviews
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task<Review> GetByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetByTutorIdAsync(Guid tutorId);
        Task<Review> GetByBookingIdAsync(Guid bookingId);
        Task<double> GetAverageRatingByTutorIdAsync(Guid tutorId);
        Task<IEnumerable<(Guid TutorId, double AverageRating, int ReviewCount)>> GetTopTutorsByRatingAsync(int count);
    }
}
