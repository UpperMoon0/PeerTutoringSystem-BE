using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Profile_Bio
{
    public interface IProfileRepository
    {
        Task<UserBio> GetByIdAsync(int profileId);
        Task<UserBio> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserBio profile);
        Task UpdateAsync(UserBio profile);
    }
}
