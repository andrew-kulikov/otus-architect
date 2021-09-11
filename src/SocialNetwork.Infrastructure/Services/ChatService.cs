using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Services;

namespace SocialNetwork.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatMemberRepository _chatMemberRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IChatRepository _chatRepository;

        public ChatService(
            IChatRepository chatRepository,
            IChatMemberRepository chatMemberRepository,
            IChatMessageRepository chatMessageRepository)
        {
            _chatRepository = chatRepository;
            _chatMemberRepository = chatMemberRepository;
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<ICollection<ChatMember>> GetUserChatsAsync(long userId)
        {
            var userChats = await _chatMemberRepository.GetUserChatsAsync(userId);
            var memberLoadingTasks = userChats.Select(c => _chatMemberRepository.GetChatMembersAsync(c.ChatId));

            var peers = await Task.WhenAll(memberLoadingTasks);

            return peers
                .SelectMany(members => members)
                .Where(member => member.UserId != userId)
                .ToList();
        }

        public async Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize) => await _chatMessageRepository.GetMessagesAsync(chatId, page, pageSize);

        public async Task CreateMessageAsync(ChatMessage message)
        {
            await _chatMessageRepository.CreateMessageAsync(message);
        }

        public async Task CreateChatAsync(long userId, long peerId)
        {
            var chat = new Chat {IsPersonal = true};
            var createdChat = await _chatRepository.CreateChatAsync(chat);

            await _chatMemberRepository.AddMemberAsync(new ChatMember
            {
                ChatId = createdChat.Id,
                UserId = userId
            });

            await _chatMemberRepository.AddMemberAsync(new ChatMember
            {
                ChatId = createdChat.Id,
                UserId = peerId
            });
        }
    }
}