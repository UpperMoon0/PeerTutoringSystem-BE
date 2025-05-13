using PeerTutoringSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces
{
    public interface IProfileRepository
    {
        Task<Profile> GetByIdAsync(int profileId);
        Task<Profile> GetByUserIdAsync(Guid userId);
        Task AddAsync(Profile profile);
        Task UpdateAsync(Profile profile);
    }
}
