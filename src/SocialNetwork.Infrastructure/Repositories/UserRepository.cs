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

        public async Task<ICollection<User>> GetAllUsersAsync()
        {
            const string sql = @"select * from Users;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var users = await connection.QueryAsync<User>(sql);

                return users.ToList();
            }
        }

        public async Task AddUserAsync(User user)
        {
            const string sql = @"insert into Users (Username, Email, PasswordHash, RegisteredAt) values (@Username, @Email, @PasswordHash, @RegisteredAt);";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                await connection.ExecuteAsync(sql, user);
            }
        }
    }
}