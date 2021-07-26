using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Messages
{
    public class PostCreatedMessage
    {
        public UserPost Post { get; set; }
    }
}