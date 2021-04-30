using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IFriendshipRepository
    {
        Task<ICollection<Friendship>> GetFriendsAsync(long userId);
        Task AddAsync(Friendship friendship);
        Task<Friendship> GetAsync(long requesterId, long addresseeId);
        Task UpdateStatusAsync(long requesterId, long addresseeId, FriendshipStatus status);
    }
}