using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string username);
        Task<ICollection<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
    }
}