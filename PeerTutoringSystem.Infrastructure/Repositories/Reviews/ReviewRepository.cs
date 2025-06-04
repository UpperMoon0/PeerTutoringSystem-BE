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
    }
}
