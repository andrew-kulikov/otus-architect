using System.Collections.Generic;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Messages
{
    public class UpdateFeedMessage
    {
        public UserPost Post { get; set; }
        public ICollection<long> UserIds { get; set; }
    }
}