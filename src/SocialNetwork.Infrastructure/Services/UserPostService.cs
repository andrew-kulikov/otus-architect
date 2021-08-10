using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Messages;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Infrastructure.Caching;
using SocialNetwork.Infrastructure.Publishers;

namespace SocialNetwork.Infrastructure.Services
{
    public class UserPostService : IUserPostService
    {
        private readonly IListCache<UserPost> _listCache;
        private readonly IMessagePublisher<PostCreatedMessage> _publisher;
        private readonly IUserPostRepository _repository;

        public UserPostService(
            IUserPostRepository repository,
            IMessagePublisher<PostCreatedMessage> publisher,
            IListCache<UserPost> listCache)
        {
            _repository = repository;
            _publisher = publisher;
            _listCache = listCache;
        }

        public async Task<ICollection<UserPost>> GetNewsFeedAsync(long userId)
        {
            var feedKey = CacheKeys.Feed.ForUser(userId);

            var posts = new List<UserPost>();//await _listCache.GetAsync(feedKey);

            if (posts == null || !posts.Any())
            {
                posts = (await _repository.GetNewsFeedAsync(userId)).ToList();

                // TODO: Add error handling
                //await _listCache.SetAsync(feedKey, posts);
            }

            return posts;
        }

        public async Task<ICollection<UserPost>> GetUserPostsAsync(long userId) => await _repository.GetUserPostsAsync(userId);

        public async Task<UserPost> GetPostAsync(long postId) => await _repository.GetPostAsync(postId);

        public async Task AddPostAsync(string text, long userId)
        {
            var post = CreatePost(text, userId);

            var savedId = await _repository.AddPostAsync(post);
            var savedPost = await _repository.GetPostAsync(savedId);

            await _publisher.PublishAsync(new PostCreatedMessage
            {
                Post = savedPost
            });
        }

        private UserPost CreatePost(string text, long userId) =>
            new()
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Text = text,
                UserId = userId
            };
    }
}