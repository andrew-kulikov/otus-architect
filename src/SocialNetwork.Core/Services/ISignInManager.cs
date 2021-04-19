using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface ISignInManager
    {
        Task SignInAsync(User user);
    }
}