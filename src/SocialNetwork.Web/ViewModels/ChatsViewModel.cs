using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class ChatsViewModel
    {
        public ICollection<Chat> ChatMembers { get; set; }
    }
}
