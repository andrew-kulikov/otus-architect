using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Core.Services
{
    public interface IFriendshipService
    {
        Task<ICollection<UserProfile>> GetFriendsAsync(long userId);
        Task AddAsync(long requesterId, long addresseeId);
        Task<Friendship> GetFriendshipAsync(long requesterId, long addresseeId);
    }

    public class FriendshipService: IFriendshipService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        public FriendshipService(IUserProfileRepository userProfileRepository, IFriendshipRepository friendshipRepository)
        {
            _userProfileRepository = userProfileRepository;
            _friendshipRepository = friendshipRepository;
        }

        public Task<ICollection<UserProfile>> GetFriendsAsync(long userId)
        {
            throw new NotImplementedException();
        }

        public async Task AddAsync(long requesterId, long addresseeId)
        {
            var requestedProfile = await _userProfileRepository.GetUserProfileAsync(addresseeId);
            if (requestedProfile == null) throw new ArgumentException($"User with id {addresseeId} does not exist");

            var friendship = new Friendship
            {
                AddresseeId = addresseeId,
                RequesterId = requesterId,
                Created = DateTime.UtcNow,
                Status = FriendshipStatus.RequestSent
            };

            await _friendshipRepository.AddAsync(friendship);
        }

        public async Task<Friendship> GetFriendshipAsync(long requesterId, long addresseeId)
        {
            var forwardFriendship = await _friendshipRepository.GetFriendshipAsync(requesterId, addresseeId);
            var backwardFriendship = await _friendshipRepository.GetFriendshipAsync(addresseeId, requesterId);

            return forwardFriendship ?? backwardFriendship;
        }
    }
}
