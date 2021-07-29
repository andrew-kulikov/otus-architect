using System;
using System.Threading.Tasks;
using MassTransit;
using SocialNetwork.Core.Messages;

namespace SocialNetwork.Infrastructure.Publishers
{
    public interface IMessagePublisher<in T>
    {
        Task PublishAsync(T message);
    }

    public class PostCreatedPublisher : IMessagePublisher<PostCreatedMessage>
    {
        private readonly IBus _bus;

        public PostCreatedPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task PublishAsync(PostCreatedMessage message)
        {
            await _bus.Publish(message);
        }
    }
}