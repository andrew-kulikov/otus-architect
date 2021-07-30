using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Messages;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;
using SocialNetwork.Infrastructure.Publishers;

namespace SocialNetwork.Infrastructure.Services
{
    public class UserPostService : IUserPostService
    {
        private readonly IUserPostRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessagePublisher<PostCreatedMessage> _publisher;

        public UserPostService(
            IUserPostRepository repository, 
            IUnitOfWork unitOfWork, 
            IMessagePublisher<PostCreatedMessage> publisher)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task<ICollection<UserPost>> GetNewsFeedAsync(long userId) => 
            await _repository.GetNewsFeedAsync(userId);

        public async Task<ICollection<UserPost>> GetUserPostsAsync(long userId) => 
            await _repository.GetUserPostsAsync(userId);

        public async Task<UserPost> GetPostAsync(long postId) => 
            await _repository.GetPostAsync(postId);

        public async Task AddPostAsync(string text, long userId)
        {
            var post = new UserPost
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Text = text,
                UserId = userId
            };

            // TODO: Return new post with id
            post.Id = await _repository.AddPostAsync(post);

            await _publisher.PublishAsync(new PostCreatedMessage
            {
                Post = post
            });
        }
    }
}