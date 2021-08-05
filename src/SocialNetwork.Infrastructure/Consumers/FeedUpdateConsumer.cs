using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Messages;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.Caching;
using SocialNetwork.Infrastructure.Extensions;

namespace SocialNetwork.Infrastructure.Consumers
{
    public class FeedUpdateConsumer : IConsumer<UpdateFeedMessage>
    {
        private readonly IListCache<UserPost> _listCache;
        private readonly IUserPostRepository _userPostRepository;

        public FeedUpdateConsumer(IListCache<UserPost> listCache, IUserPostRepository userPostRepository)
        {
            _listCache = listCache;
            _userPostRepository = userPostRepository;
        }

        public async Task Consume(ConsumeContext<UpdateFeedMessage> context)
        {
            foreach (var userId in context.Message.UserIds)
            {
                var feedKey = CacheKeys.Feed.ForUser(userId);

                var posts = await GetPostsAsync(userId, feedKey, context.Message.Post);

                await _listCache.SetAsync(feedKey, posts);
            }
        }

        private async Task<List<UserPost>> GetPostsAsync(long userId, string feedKey, UserPost addedPost)
        {
            var posts = await _listCache.GetAsync(feedKey);
            if (posts == null || !posts.Any()) return await GetPosts(userId);

            return posts.With(addedPost);
        }

        private async Task<List<UserPost>> GetPosts(long userId)
        {
            var posts = await _userPostRepository.GetNewsFeedAsync(userId);

            return posts.ToList();
        }
    }
}