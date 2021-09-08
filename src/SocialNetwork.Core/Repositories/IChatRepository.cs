using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Repositories
{
    public interface IChatMessageRepository
    {
        Task<ICollection<ChatMessage>> GetMessages(long chatId, int page, int pageSize);
        Task CreateMessage(ChatMessage message);
    }

    public interface IChatMemberRepository
    {
        Task<ICollection<ChatMember>> GetUserChats(long userId);
        Task AddMember(ChatMember member);
    }

    public interface IChatRepository
    {
        Task CreateChat(Chat chat);
    }
}