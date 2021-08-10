using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class UserPostRepository : IUserPostRepository
    {
        private readonly DbContext _dbContext;

        public UserPostRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<UserPost>> GetUserPostsAsync(long userId)
        {
            const string sql = @"select * from UserPost where UserId = @UserId;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<UserPost>(sql, new {UserId = userId});

                return result.ToList();
            });
        }

        public async Task<UserPost> GetPostAsync(long postId)
        {
            const string sql = @"
                select * from UserPost
                join UserProfile on UserPost.UserId = UserProfile.UserId
                where Id = @PostId;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<UserPost, UserProfile, UserPost>(sql,
                    (post, profile) =>
                    {
                        post.UserProfile = profile;

                        return post;
                    },
                    new {PostId = postId},
                    splitOn: "Id,UserId");

                return result.FirstOrDefault();
            });
        }

        public async Task<long> AddPostAsync(UserPost post)
        {
            const string sql = @"
                insert into UserPost (UserId, Text, Created, Updated) values (@UserId, @Text, @Created, @Updated);
                select last_insert_id();";

            return await _dbContext.ExecuteQueryAsync(async connection => 
                await connection.QueryFirstOrDefaultAsync<long>(sql, new {post.UserId, post.Text, post.Created, post.Updated}), true);
        }

        public async Task<ICollection<UserPost>> GetNewsFeedAsync(long userId)
        {
            const string sql =
                @"select UserPost.*, UserProfile.* from UserPost
                    join UserProfile on UserPost.UserId = UserProfile.UserId
                    left join Friendship Outgoing on Outgoing.RequesterId = UserProfile.UserId
                    left join Friendship Incoming on Incoming.AddresseeId = UserProfile.UserId
                    where (Outgoing.AddresseeId = @UserId and Outgoing.Status = 1) or (Incoming.RequesterId = @UserId and (Incoming.Status = 1 or Incoming.Status = 0))
                    limit 1000;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<UserPost, UserProfile, UserPost>(sql,
                    (post, profile) =>
                    {
                        post.UserProfile = profile;

                        return post;
                    },
                    new {UserId = userId},
                    splitOn: "Id,UserId");

                return result.ToList();
            });
        }
    }
}