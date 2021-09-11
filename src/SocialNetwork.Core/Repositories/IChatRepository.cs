using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IChatMessageRepository
    {
        Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize);
        Task CreateMessageAsync(ChatMessage message);
    }

    public interface IChatMemberRepository
    {
        Task<ICollection<ChatMember>> GetUserChatsAsync(long userId);
        Task<ICollection<ChatMember>> GetChatMembersAsync(long chatId);
        Task AddMemberAsync(ChatMember member);
    }

    public interface IChatRepository
    {
        Task<Chat> CreateChatAsync(Chat chat);
    }
}