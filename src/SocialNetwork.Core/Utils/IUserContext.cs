using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Utils
{
    public interface IUserContext
    {
        User CurrentUser { get; set; }
    }

    public class UserContext: IUserContext
    {
        public User CurrentUser { get; set; }
    }
}