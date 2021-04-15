using System;
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
        private readonly SqlConnectionFactory _connectionFactory;

        public UserRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User> GetUserAsync(string username)
        {
            const string sql = @"select * from User where Username = @Username;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var users = await connection.QueryAsync<User>(sql, new { Username = username });

                return users.First();
            }
        }

        public async Task<ICollection<User>> GetAllUsersAsync()
        {
            const string sql = @"select * from User;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var users = await connection.QueryAsync<User>(sql);

                return users.ToList();
            }
        }

        public async Task AddUserAsync(User user)
        {
            const string sql = @"insert into User (Username, Email, PasswordHash, RegisteredAt) values (@Username, @Email, @PasswordHash, @RegisteredAt);";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                await connection.ExecuteAsync(sql, user);
            }
        }
    }
}