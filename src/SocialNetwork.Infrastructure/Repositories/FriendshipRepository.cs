using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class FriendshipRepository: IFriendshipRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public FriendshipRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ICollection<UserProfile>> GetFriendsAsync(long userId)
        {
            //const string sql =
            //    @"select User.*, UserProfile.* 
            //      from User
            //      left join UserProfile on UserProfile.UserId = User.Id
            //      where User.Username = @Username;";

            //using (var connection = _connectionFactory.CreateConnection())
            //{
            //    connection.Open();

            //    var users = await connection.QueryAsync<User, UserProfile, User>(sql,
            //        (user, profile) =>
            //        {
            //            user.Profile = profile;
            //            return user;
            //        },
            //        new { Username = username },
            //        splitOn: "UserId");

            //    return users.First();
            //}

            throw new NotImplementedException();
        }

        public async Task AddAsync(Friendship friendship)
        {
            const string sql = @"insert into Friendship (RequesterId, AddresseeId, Created, Status) values (@RequesterId, @AddresseeId, @Created, @Status);";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                await connection.ExecuteAsync(sql, friendship);
            }
        }

        public async Task<Friendship> GetFriendshipAsync(long requesterId, long addresseeId)
        {
            const string sql = @"select * from Friendship where RequesterId = @RequesterId and AddresseeId = @AddresseeId;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var result = await connection.QueryAsync<Friendship>(sql, new { RequesterId = requesterId, AddresseeId = addresseeId});

                return result.FirstOrDefault();
            }
        }
    }
}
