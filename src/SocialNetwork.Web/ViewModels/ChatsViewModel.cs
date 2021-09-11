using System.Collections.Generic;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class ChatsViewModel
    {
        public ICollection<ChatMember> Chats { get; set; }
    }
}