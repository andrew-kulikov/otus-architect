using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.Infrastructure.Repositories
{
    public class ChatMemberRepository : IChatMemberRepository
    {
        private readonly DbContext _dbContext;

        public ChatMemberRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ChatMember>> GetUserChatsAsync(long userId)
        {
            const string sql = @"select * from ChatMember where UserId = @UserId";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<ChatMember>(sql, new {UserId = userId});

                return result.ToList();
            });
        }

        public async Task<ICollection<ChatMember>> GetChatMembersAsync(long chatId)
        {
            const string sql = @"select * from ChatMember 
                                 join UserProfile on UserProfile.UserId = ChatMember.UserId
                                 where ChatId = @ChatId";

            return await _dbContext.ExecuteQueryAsync(async connection =>
            {
                var result = await connection.QueryAsync<ChatMember, UserProfile, ChatMember>(
                    sql,
                    (chatMember, profile) =>
                    {
                        chatMember.UserProfile = profile;

                        return chatMember;
                    },
                    new { ChatId = chatId },
                    splitOn: "UserId");

                return result.ToList();
            });
        }

        public async Task AddMemberAsync(ChatMember member)
        {
            const string sql = @"insert into ChatMember (ChatId, UserId) values (@ChatId, @UserId)";

            await _dbContext.AddCommandAsync(connection => connection.ExecuteAsync(sql, new
            {
                member.ChatId,
                member.UserId
            }));
        }
    }
}