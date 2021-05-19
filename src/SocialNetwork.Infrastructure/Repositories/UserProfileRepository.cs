using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly DbContext _dbContext;

        public UserProfileRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserProfileAsync(User user, UserProfile userProfile)
        {
            const string sql = @"insert into UserProfile (UserId, FirstName, LastName, Age, Interests, City) values (@UserId, @FirstName, @LastName, @Age, @Interests, @City);";

            userProfile.UserId = user.Id;

            await _dbContext.AddCommandAsync(connection => connection.ExecuteAsync(sql, userProfile));
        }

        public async Task<ICollection<UserProfile>> GetAllUserProfilesAsync(int page, int pageSize)
        {
            var sql = @$"select * from UserProfile limit {pageSize} offset {page * pageSize}";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var profiles = await connection.QueryAsync<UserProfile>(sql);

                return profiles.ToList();
            });
        }

        public async Task<UserProfile> GetUserProfileAsync(long userId)
        {
            const string sql =
                @"select * 
                  from UserProfile
                  where UserId = @UserId";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var profiles = await connection.QueryAsync<UserProfile>(sql, new { UserId = userId });

                return profiles.First();
            });
        }
    }
}