using SocialNetwork.Core.Entities;

namespace SocialNetwork.Infrastructure.HubBackplane
{
    public class UserPostUpdateDto
    {
        public UserPost Post { get; set; }
        public long TargetUserId { get; set; }
    }
}