using System.Threading.Tasks;
using MassTransit;
using SocialNetwork.Core.Messages;

namespace SocialNetwork.Infrastructure.Consumers
{
    public class FeedUpdateConsumer : IConsumer<UpdateFeedMessage>
    {
        public Task Consume(ConsumeContext<UpdateFeedMessage> context) => Task.CompletedTask;
    }
}