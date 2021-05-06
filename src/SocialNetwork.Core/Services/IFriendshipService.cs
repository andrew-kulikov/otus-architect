using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface IFriendshipService
    {
        Task<ICollection<Friendship>> GetFriendsAsync(long userId);
        Task AddAsync(long requesterId, long addresseeId);
        Task<Friendship> GetFriendshipAsync(long requesterId, long addresseeId);
        Task AcceptAsync(long requesterId, long addresseeId);
        Task RemoveAsync(long userId, long friendId);
    }
}
