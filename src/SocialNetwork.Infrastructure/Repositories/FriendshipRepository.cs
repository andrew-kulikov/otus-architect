using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public FriendshipRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ICollection<Friendship>> GetFriendsAsync(long userId)
        {
            const string sql =
                @"select Friendship.*, Requester.*,  Addressee.*
                    from Friendship
                    join UserProfile Requester on Requester.UserId = Friendship.RequesterId
                    join UserProfile Addressee on Addressee.UserId = Friendship.AddresseeId
                    where Friendship.RequesterId = @UserId or Friendship.AddresseeId = @UserId;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var result = await connection.QueryAsync<Friendship, UserProfile, UserProfile, Friendship>(sql,
                    (friendship, requester, addressee) =>
                    {
                        friendship.Requester = requester;
                        friendship.Addressee = addressee;

                        return friendship;
                    },
                    new {UserId = userId},
                    splitOn: "UserId");

                return result.ToList();
            }
        }

        public async Task AddAsync(Friendship friendship)
        {
            const string sql =
                @"insert into Friendship (RequesterId, AddresseeId, Created, Status) values (@RequesterId, @AddresseeId, @Created, @Status);";

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

                var result = await connection.QueryAsync<Friendship>(sql, new {RequesterId = requesterId, AddresseeId = addresseeId});

                return result.FirstOrDefault();
            }
        }

        public async Task UpdateStatusAsync(long requesterId, long addresseeId, FriendshipStatus status)
        {
            const string sql = @"update Friendship set Status = @Status where RequesterId = @RequesterId and AddresseeId = @AddresseeId;";

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                await connection.ExecuteAsync(sql, new {Status = status, RequesterId = requesterId, AddresseeId = addresseeId});
            }
        }
    }
}