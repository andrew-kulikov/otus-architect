using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly DbContext _dbContext;

        public ChatRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            const string sql = @"insert into Chat (IsPersonal) values (@IsPersonal); select last_insert_id();";

            var id = await _dbContext.ExecuteQueryAsync(
                connection => connection.QuerySingleAsync<long>(sql, new {chat.IsPersonal}), true);

            chat.Id = id;

            return chat;
        }
    }
}