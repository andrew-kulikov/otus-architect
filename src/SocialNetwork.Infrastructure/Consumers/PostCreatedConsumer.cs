using System.Collections.Generic;
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

            var friendshipRelations = await _friendshipRepository.GetAcceptedFriendsAsync(userId);

            foreach (var subscriberBatch in friendshipRelations.Batch(1000))
            {
                var updateMessage = BuildUpdateMessage(subscriberBatch, context.Message.Post);

                await context.Publish(updateMessage);
            }
        }

        private static UpdateFeedMessage BuildUpdateMessage(IEnumerable<Friendship> subscriberBatch, UserPost post)
        {
            var subscriberIds = subscriberBatch.Select(friendship => GetOtherUserId(friendship, post.UserId)).ToList();

            return new UpdateFeedMessage
            {
                Post = post,
                UserIds = subscriberIds
            };
        }

        private static long GetOtherUserId(Friendship friendship, long userId) =>
            friendship.AddresseeId == userId
                ? friendship.RequesterId
                : friendship.AddresseeId;
    }
}