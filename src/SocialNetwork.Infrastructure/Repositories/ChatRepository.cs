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

        public async Task CreateChat(Chat chat)
        {
            const string sql = @"insert into Chat (IsPersonal) values (@IsPersonal)";

            await _dbContext.AddCommandAsync(connection => connection.ExecuteAsync(sql, new {chat.IsPersonal}));
        }
    }
}