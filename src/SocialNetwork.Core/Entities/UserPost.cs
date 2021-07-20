using System;

namespace SocialNetwork.Core.Entities
{
    public class UserPost
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public UserProfile UserProfile { get; set; }
    }
}