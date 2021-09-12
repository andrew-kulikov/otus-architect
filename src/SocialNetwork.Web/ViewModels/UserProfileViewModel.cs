using System;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Web.ViewModels
{
    public class UserProfileViewModel
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }

        public bool IsFriendshipInitiated { get; set; }
        public bool IsUserRequester { get; set; }
        public DateTime FriendshipCreated { get; set; }
        public FriendshipStatus FriendshipStatus { get; set; }

        public bool ChatCreated { get; set; }
        public long? ChatId { get; set; }
    }
}