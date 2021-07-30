﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IUserPostRepository
    {
        Task<ICollection<UserPost>> GetNewsFeedAsync(long userId);
        Task<ICollection<UserPost>> GetUserPostsAsync(long userId);
        Task<UserPost> GetPostAsync(long postId);
        Task<long> AddPostAsync(UserPost post);
    }
}