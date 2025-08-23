using PeerTutoringSystem.Domain.Entities.Profile_Bio;
using System;
using System.Threading.Tasks;

namespace PeerTutoringSystem.Domain.Interfaces.Profile_Bio
{
    public interface IUserBioRepository
    {
        Task<UserBio> GetByIdAsync(int bioId);
        Task<UserBio> GetByUserIdAsync(Guid userId); 
        Task AddAsync(UserBio userBio);
        Task UpdateAsync(UserBio userBio);
    }
}