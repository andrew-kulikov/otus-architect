using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly DbContext _dbContext;

        public ChatMessageRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ChatMessage>> GetMessages(long chatId, int page, int pageSize)
        {
            const string sql = @"select * from ChatMessage where ChatId = @ChatId order by ChatLocalId desc limit @Limit offset @Offset";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<ChatMessage>(sql, new
                {
                    ChatId = chatId,
                    Limit = pageSize,
                    Offset = pageSize * page
                });

                return result.ToList();
            });
        }

        public async Task CreateMessage(ChatMessage message)
        {
            const string sql = @"insert into ChatMessage (ChatId, SenderId, ChatLocalId, Text, Created, Updated, IsDeleted)
                                 values (@ChatId, @SenderId, @ChatLocalId, @Text, @Created, @Updated, @IsDeleted)";

            await _dbContext.AddCommandAsync(connection => connection.ExecuteAsync(sql, message));
        }
    }
}