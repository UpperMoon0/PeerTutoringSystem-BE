using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Entities.Booking
{
    public record BookingFilter(
        int Page = 1,
        int PageSize = 10,
        string? Status = null,
        Guid? SkillId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null);
}
