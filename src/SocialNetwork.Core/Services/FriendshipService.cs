using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Core.Services
{
    public class FriendshipService: IFriendshipService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FriendshipService(
            IUserProfileRepository userProfileRepository, 
            IFriendshipRepository friendshipRepository, 
            IUnitOfWork unitOfWork)
        {
            _userProfileRepository = userProfileRepository;
            _friendshipRepository = friendshipRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ICollection<Friendship>> GetFriendsAsync(long userId)
        {
            return await _friendshipRepository.GetFriendsAsync(userId);
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

            await _unitOfWork.CommitAsync();
        }

        public async Task<Friendship> GetFriendshipAsync(long requesterId, long addresseeId)
        {
            var forwardFriendship = await _friendshipRepository.GetAsync(requesterId, addresseeId);
            var backwardFriendship = await _friendshipRepository.GetAsync(addresseeId, requesterId);

            return forwardFriendship ?? backwardFriendship;
        }

        public async Task AcceptAsync(long requesterId, long addresseeId)
        {
            var forwardFriendship = await _friendshipRepository.GetAsync(requesterId, addresseeId);
            if (forwardFriendship == null) throw new Exception("Friendship not found");

            await _friendshipRepository.UpdateStatusAsync(requesterId, addresseeId, FriendshipStatus.RequestAccepted);

            await _unitOfWork.CommitAsync();
        }

        public async Task RemoveAsync(long userId, long friendId)
        {
            var forwardFriendship = await _friendshipRepository.GetAsync(userId, friendId);
            if (forwardFriendship != null)
            {
                await _friendshipRepository.UpdateStatusAsync(userId, friendId, FriendshipStatus.RemovedByRequester);
                await _unitOfWork.CommitAsync();

                return;
            }

            var backwardFriendship = await _friendshipRepository.GetAsync(friendId, userId);
            if (backwardFriendship != null)
            {
                await _friendshipRepository.UpdateStatusAsync(friendId, userId, FriendshipStatus.RemovedByAddressee);
                await _unitOfWork.CommitAsync();

                return;
            }

            throw new Exception("Friendship not found");
        }
    }
}