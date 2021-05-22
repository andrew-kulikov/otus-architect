using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IUserProfileRepository
    {
        Task AddUserProfileAsync(User user, UserProfile userProfile);
        Task<ICollection<UserProfile>> GetAllUserProfilesAsync(int page, int pageSize);
        Task<ICollection<UserProfile>> SearchUserProfilesAsync(string query);
        Task<UserProfile> GetUserProfileAsync(long userId);
    }
}