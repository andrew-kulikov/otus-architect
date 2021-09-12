using System.Collections.Generic;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class ChatViewModel
    {
        public long ChatId { get; set; }
        public long UserId { get; set; }
        public UserProfile Peer { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public ICollection<ChatMessage> Messages { get; set; }
    }
}