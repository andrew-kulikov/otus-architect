using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddUserProfileAsync(User user, UserProfile userProfile);
        Task<ICollection<UserProfile>> GetAllUserProfilesAsync();
    }
}