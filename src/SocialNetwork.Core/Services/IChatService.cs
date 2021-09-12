using System.Collections.Generic;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Services
{
    public interface IChatService
    {
        Task<ICollection<ChatMember>> GetUserChatsAsync(long userId);
        Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize);
        Task CreateMessageAsync(string text, long chatId, long userId);
        Task CreateChatAsync(long userId, long peerId);
        Task<ChatMember> FindChatAsync(long userId, long peerId);
        Task<ChatMember> FindPeerAsync(long chatId, long userId);
    }
}