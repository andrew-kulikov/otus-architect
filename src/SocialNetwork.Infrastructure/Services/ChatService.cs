using System;
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
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(
            IChatRepository chatRepository,
            IChatMemberRepository chatMemberRepository,
            IChatMessageRepository chatMessageRepository,
            IUserProfileRepository userProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _chatRepository = chatRepository;
            _chatMemberRepository = chatMemberRepository;
            _chatMessageRepository = chatMessageRepository;
            _unitOfWork = unitOfWork;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<ICollection<ChatMember>> GetUserChatsAsync(long userId)
        {
            var userChats = await _chatMemberRepository.GetUserChatsAsync(userId);
            var memberLoadingTasks = userChats.Select(c => _chatMemberRepository.GetChatMembersAsync(c.ChatId));

            var peers = await Task.WhenAll(memberLoadingTasks);

            return peers
                .Select(members => members.FirstOrDefault(member => member.UserId != userId))
                .ToList();
        }

        public async Task<ICollection<ChatMessage>> GetMessagesAsync(long chatId, int page, int pageSize) => 
            await _chatMessageRepository.GetMessagesAsync(chatId, page, pageSize);

        // TODO: Move to consumer
        public async Task CreateMessageAsync(string text, long chatId, long userId)
        {
            var lastLocalId = await GetLastMessageId(chatId);

            var message = new ChatMessage
            {
                Text = text,
                ChatId = chatId,
                SenderId = userId,
                ChatLocalId = lastLocalId + 1,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                IsDeleted = false
            };

            await _chatMessageRepository.CreateMessageAsync(message);
            await _unitOfWork.CommitAsync();
        }

        private async Task<int> GetLastMessageId(long chatId)
        {
            // TODO: Load from cache
            var lastMessages = await _chatMessageRepository.GetMessagesAsync(chatId, 0, 1);
        
            return lastMessages.FirstOrDefault()?.ChatLocalId ?? 0;
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

            await _unitOfWork.CommitAsync();
        }

        public async Task<ChatMember> FindChatAsync(long userId, long peerId) =>
            await _chatMemberRepository.FindChatAsync(userId, peerId);

        public async Task<ChatMember> FindPeerAsync(long chatId, long userId)
        {
            var peerMember = await _chatMemberRepository.FindPeerAsync(chatId, userId);
            var peer = await _userProfileRepository.GetUserProfileAsync(peerMember.UserId);

            peerMember.UserProfile = peer;

            return peerMember;
        }
    }
}