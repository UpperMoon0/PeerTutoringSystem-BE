using PeerTutoringSystem.Application.DTOs.Reviews;


namespace PeerTutoringSystem.Application.Interfaces.Reviews
{
    public interface IReviewService
    {
        Task<int> CreateReviewAsync(CreateReviewDto dto);
        Task<ReviewDto> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetReviewsByTutorIdAsync(Guid tutorId);
        Task<double> GetAverageRatingByTutorIdAsync(Guid tutorId);
        Task<IEnumerable<TutorRatingDto>> GetTopTutorsByRatingAsync(int count);
    }
}
