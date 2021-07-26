using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Messages;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.Extensions;

namespace SocialNetwork.Infrastructure.Consumers
{
    public class PostCreatedConsumer : IConsumer<PostCreatedMessage>
    {
        private readonly IFriendshipRepository _friendshipRepository;

        public PostCreatedConsumer(IFriendshipRepository friendshipRepository)
        {
            _friendshipRepository = friendshipRepository;
        }

        public async Task Consume(ConsumeContext<PostCreatedMessage> context)
        {
            var userId = context.Message.Post.UserId;
            var friendshipRelations = await _friendshipRepository.GetFriendsAsync(userId);

            var subscriberBatches = friendshipRelations
                .Where(r => r.Status == FriendshipStatus.RequestAccepted || r.Status == FriendshipStatus.RequestSent)
                .Batch(100);

            foreach (var subscriberBatch in subscriberBatches)
            {
                var subscriberIds = subscriberBatch.Select(friendship => GetOtherUserId(friendship, userId)).ToList();
                
                await context.Send(new UpdateFeedMessage
                {
                    Post = context.Message.Post,
                    UserIds = subscriberIds
                });
            }
        }

        private static long GetOtherUserId(Friendship friendship, long userId) =>
            friendship.AddresseeId == userId
                ? friendship.RequesterId
                : friendship.AddresseeId;
    }
}