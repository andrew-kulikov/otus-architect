using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface IUserPostService
    {
        Task<ICollection<UserPost>> GetNewsFeedAsync(long userId);
        Task<UserPost> GetPostAsync(long postId);
        Task AddPostAsync(string text, long userId);
    }
}