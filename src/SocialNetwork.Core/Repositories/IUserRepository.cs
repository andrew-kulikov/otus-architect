using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
    }
}