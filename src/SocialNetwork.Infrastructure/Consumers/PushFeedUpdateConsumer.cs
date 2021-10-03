using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SocialNetwork.Core.Messages;
using SocialNetwork.Infrastructure.HubBackplane;

namespace SocialNetwork.Infrastructure.Consumers
{
    public class PushFeedUpdateConsumer : IConsumer<UpdateFeedMessage>
    {
        private readonly ILogger<FeedUpdateConsumer> _logger;
        private readonly INewsHubBackplane _hubBackplane;

        public PushFeedUpdateConsumer(ILogger<FeedUpdateConsumer> logger, INewsHubBackplane hubBackplane)
        {
            _logger = logger;
            _hubBackplane = hubBackplane;
        }

        public async Task Consume(ConsumeContext<UpdateFeedMessage> context)
        {
            foreach (var userId in context.Message.UserIds)
            {
                _logger.LogInformation($"Pushing post {context.Message.Post.Text} to user #{userId}");

                await _hubBackplane.PublishAsync(context.Message.Post, userId);
            }
        }
    }
}