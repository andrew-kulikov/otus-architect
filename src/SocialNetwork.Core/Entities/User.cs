using System;
using System.Collections.Generic;
using System.Text;

namespace SocialNetwork.Core.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime RegisteredAt { get; set; }

        public UserProfile Profile { get; set; }
    }
}
