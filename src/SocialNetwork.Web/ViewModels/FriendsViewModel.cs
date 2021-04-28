using System.Collections.Generic;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class FriendsViewModel
    {
        public ICollection<UserProfile> Friends { get; set; }
        public ICollection<Friendship> PendingRequests { get; set; }
        public ICollection<Friendship> IncomingRequests { get; set; }
    }
}