using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface IAuthenticationService
    {
        Task RegisterAsync(User user, string password);
        Task<User> LoginAsync(string username, string password);
    }
}