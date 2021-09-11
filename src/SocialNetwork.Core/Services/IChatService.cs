using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface IChatService
    {
        Task<ICollection<ChatMember>> GetUserChatsAsync(long userId);
        Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize);
        Task CreateMessageAsync(ChatMessage message);
        Task CreateChatAsync(long userId, long peerId);
    }
}