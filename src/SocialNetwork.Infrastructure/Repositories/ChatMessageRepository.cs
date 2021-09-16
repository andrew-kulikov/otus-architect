﻿using System.Collections.Generic;
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
        private readonly MessagesDbContext _dbContext;

        public ChatMessageRepository(MessagesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize)
        {
            var sql = @$"select /* {ResolveShard(chatId)} */ * from ChatMessage where ChatId = @ChatId order by ChatLocalId desc limit @Limit offset @Offset";

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

        public async Task CreateMessageAsync(ChatMessage message)
        {
            var sql = @$"insert /* {ResolveShard(message.ChatId)} */ into ChatMessage (ChatId, SenderId, ChatLocalId, Text, Created, Updated, IsDeleted)
                         values (@ChatId, @SenderId, @ChatLocalId, @Text, @Created, @Updated, @IsDeleted)";

            await _dbContext.ExecuteQueryAsync(connection => connection.ExecuteAsync(sql, message), true);
        }

        private static string ResolveShard(long chatId) =>
            $"shard{ResolveShardId(chatId):D4}";

        private static int ResolveShardId(long chatId)
        {
            if (chatId <= 3) return 0;
            return 1;
        }
    }
}