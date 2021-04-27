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
        private readonly SqlConnectionFactory _connectionFactory;

        public UserProfileRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task AddUserProfileAsync(User user, UserProfile userProfile)
        {
            const string sql = @"insert into UserProfile (UserId, FirstName, LastName, Age, Interests, City) values (@UserId, @FirstName, @LastName, @Age, @Interests, @City);";

            userProfile.UserId = user.Id;

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                await connection.ExecuteAsync(sql, userProfile);
            }
        }

        public async Task<ICollection<UserProfile>> GetAllUserProfilesAsync()
        {
            const string sql = @"select * from UserProfile";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var profiles = await connection.QueryAsync<UserProfile>(sql);

                return profiles.ToList();
            }
        }

        public async Task<UserProfile> GetUserProfileAsync(long userId)
        {
            const string sql =
                @"select * 
                  from UserProfile
                  where UserId = @UserId";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var profiles = await connection.QueryAsync<UserProfile>(sql, new { UserId = userId });

                return profiles.First();
            }
        }
    }
}