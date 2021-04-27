using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Utils
{
    public class UserContext: IUserContext
    {
        public User CurrentUser { get; set; }
    }
}