using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialNetwork.Core.Entities;
using SocialNetwork.Infrastructure.HubBackplane;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace SocialNetwork.Web.Hubs
{
    public class RedisNewsHubBackplane : INewsHubBackplane
    {
        private readonly IHubContext<NewsFeedHub> _hubContext;
        private readonly ILogger<RedisNewsHubBackplane> _logger;
        private readonly IRedisCacheClient _redisCacheClient;

        public RedisNewsHubBackplane(IRedisCacheClient redisCacheClient, IHubContext<NewsFeedHub> hubContext, ILogger<RedisNewsHubBackplane> logger)
        {
            _redisCacheClient = redisCacheClient;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SubscribeToUserFeedAsync(long userId)
        {
            _logger.LogInformation($"User {userId} subscribed to news feed");

            var subscriber = _redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetSubscriber();
            await subscriber.SubscribeAsync($"feed-{userId}", PushPost);
        }

        public async Task UnsubscribeFromUserFeedAsync(long userId)
        {
            _logger.LogInformation($"User {userId} unsubscribed from news feed");

            var subscriber = _redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetSubscriber();

            await subscriber.UnsubscribeAsync($"feed-{userId}", PushPost);
        }

        public async Task PublishAsync(UserPost post, long targetUserId)
        {
            _logger.LogInformation("Pushing post");

            var updateDto = new UserPostUpdateDto
            {
                Post = post,
                TargetUserId = targetUserId
            };

            var message = JsonConvert.SerializeObject(updateDto);

            var subscriber = _redisCacheClient.GetDbFromConfiguration().Database.Multiplexer.GetSubscriber();

            await subscriber.PublishAsync($"feed-{targetUserId}", message);
        }

        private async void PushPost(RedisChannel channel, RedisValue message)
        {
            _logger.LogInformation($"Received message {message}");

            var deserialized = JsonConvert.DeserializeObject<UserPostUpdateDto>(message.ToString());

            await _hubContext.Clients.Group($"feed-{deserialized.TargetUserId}").SendAsync("addPost", deserialized.Post);
        }
    }
}