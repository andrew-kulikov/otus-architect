using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Messages;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.Caching;

namespace SocialNetwork.Infrastructure.Consumers
{
    public class FeedUpdateConsumer : IConsumer<UpdateFeedMessage>
    {
        private readonly IListCache<UserPost> _listCache;
        private readonly IUserPostRepository _userPostRepository;
        private readonly ILogger<FeedUpdateConsumer> _logger;

        public FeedUpdateConsumer(IListCache<UserPost> listCache, IUserPostRepository userPostRepository,
            ILogger<FeedUpdateConsumer> logger)
        {
            _listCache = listCache;
            _userPostRepository = userPostRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateFeedMessage> context)
        {
            foreach (var userId in context.Message.UserIds)
            {
                _logger.LogInformation($"Adding post {context.Message.Post.Text} to user #{userId}");

                var feedKey = CacheKeys.Feed.ForUser(userId);

                if (_listCache.Any(feedKey))
                {
                    await _listCache.AddAsync(feedKey, context.Message.Post);
                }
                else
                {
                    var posts = await GetPostsAsync(userId);
                    await _listCache.SetAsync(feedKey, posts);
                }
            }
        }

        private async Task<List<UserPost>> GetPostsAsync(long userId)
        {
            var posts = await _userPostRepository.GetNewsFeedAsync(userId);

            return posts.ToList();
        }
    }
}