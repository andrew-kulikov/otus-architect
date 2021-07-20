﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;

namespace SocialNetwork.Infrastructure.Services
{
    public class UserPostService : IUserPostService
    {
        private readonly IUserPostRepository _repository;

        public UserPostService(IUserPostRepository repository)
        {
            _repository = repository;
        }

        public async Task<ICollection<UserPost>> GetNewsFeedAsync(long userId) => await _repository.GetNewsFeedAsync(userId);

        public async Task<UserPost> GetPostAsync(long postId) => await _repository.GetPostAsync(postId);

        public async Task AddPostAsync(string text, long userId)
        {
            var post = new UserPost
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Text = text,
                UserId = userId
            };

            await _repository.AddPostAsync(post);
        }
    }
}