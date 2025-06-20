using Microsoft.EntityFrameworkCore;
using PeerTutoringSystem.Domain.Entities.Reviews;
using PeerTutoringSystem.Domain.Interfaces.Reviews;
using PeerTutoringSystem.Infrastructure.Data;


namespace PeerTutoringSystem.Infrastructure.Repositories.Reviews
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<Review> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Tutor)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReviewID == reviewId);
        }

        public async Task<IEnumerable<Review>> GetByTutorIdAsync(Guid tutorId)
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Tutor)
                .Include(r => r.Booking)
                .Where(r => r.TutorID == tutorId)
                .ToListAsync();
        }

        public async Task<Review> GetByBookingIdAsync(Guid bookingId)
        {
            return await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Tutor)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.BookingID == bookingId);
        }

        public async Task<double> GetAverageRatingByTutorIdAsync(Guid tutorId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TutorID == tutorId)
                .ToListAsync();

            if (!reviews.Any())
                return 0.0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<IEnumerable<(Guid TutorId, double AverageRating, int ReviewCount)>> GetTopTutorsByRatingAsync(int count)
        {
            var tutorsWithRatings = await _context.Reviews
                .GroupBy(r => r.TutorID)
                .Select(g => new
                {
                    TutorId = g.Key,
                    AverageRating = g.Average(r => r.Rating),
                    ReviewCount = g.Count()
                })
                .OrderByDescending(t => t.AverageRating)
                .Take(count)
                .ToListAsync();

            var result = tutorsWithRatings.Select(t => (t.TutorId, t.AverageRating, t.ReviewCount)).ToList();

            if (result.Count < count)
            {
                var existingTutorIds = result.Select(r => r.TutorId).ToHashSet();
                var additionalTutors = await _context.Users
                    .Where(u => u.Role.RoleName == "Tutor" && !existingTutorIds.Contains(u.UserID))
                    .ToListAsync();

                var tutorsToAdd = additionalTutors.Take(count - result.Count);
                result.AddRange(tutorsToAdd.Select(u => (u.UserID, 0.0, 0)));
            }

            return result;
        }
    }
}
