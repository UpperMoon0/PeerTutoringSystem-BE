namespace PeerTutoringSystem.Application.DTOs.Booking
{
    // DTO cho yêu cầu phân trang và lọc
    public class BookingFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Status { get; set; } 
        public Guid? SkillId { get; set; } 
        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; } 
    }
}