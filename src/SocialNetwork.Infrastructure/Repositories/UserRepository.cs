using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _dbContext;

        public UserRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserAsync(string username)
        {
            const string sql =
                @"select User.*, UserProfile.* 
                  from User
                  left join UserProfile on UserProfile.UserId = User.Id
                  where User.Username = @Username;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var users = await connection.QueryAsync<User, UserProfile, User>(sql,
                    (user, profile) =>
                    {
                        user.Profile = profile;
                        return user;
                    },
                    new {Username = username},
                    splitOn: "UserId");

                return users.First();
            });
        }

        public async Task<ICollection<User>> GetAllUsersAsync()
        {
            const string sql = @"select * from User;";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var users = await connection.QueryAsync<User>(sql);

                return users.ToList();
            });
        }

        public async Task AddUserAsync(User user)
        {
            const string sql =
                @"insert into User (Username, Email, PasswordHash, RegisteredAt) values (@Username, @Email, @PasswordHash, @RegisteredAt);";

            await _dbContext.AddCommandAsync(connection => connection.ExecuteAsync(sql, user));
        }
    }
}