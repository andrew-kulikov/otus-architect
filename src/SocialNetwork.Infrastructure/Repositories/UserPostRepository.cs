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

        public async Task<UserPost> GetPostAsync(long postId)
        {
            const string sql = @"select * from UserPost where Id = @PostId;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
                await connection.QueryFirstOrDefaultAsync<UserPost>(sql, new {PostId = postId}));
        }

        public async Task AddPostAsync(UserPost post)
        {
            const string sql = @"insert into UserPost (UserId, Text, Created, Updated) values (@UserId, @Text, @Created, @Updated)";

            await _dbContext.AddCommandAsync(async connection => 
                await connection.ExecuteAsync(sql, new {post.UserId, post.Text, post.Created, post.Updated}));
        }

        public async Task<ICollection<UserPost>> GetNewsFeedAsync(long userId)
        {
            const string sql =
                @"select UserPost.*
                from UserPost
                join Friendship 
                join UserProfile Requester on Requester.UserId = Friendship.RequesterId
                join UserProfile Addressee on Addressee.UserId = Friendship.AddresseeId
                where (Friendship.RequesterId = @UserId and (Friendship.Status = 1 or Friendship.Status = 0)) 
	                  or Friendship.AddresseeId = @UserId and Friendship.Status = 1
                limit 1000;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<UserPost, Friendship, UserProfile, UserProfile, UserPost>(sql,
                    (post, friendship, requester, addressee) =>
                    {
                        friendship.Requester = requester;
                        friendship.Addressee = addressee;

                        return post;
                    },
                    new {UserId = userId});

                return result.ToList();
            });
        }
    }
}