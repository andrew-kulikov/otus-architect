using SocialNetwork.Core.Entities;

namespace SocialNetwork.Core.Utils
{
    public interface IUserContext
    {
        User CurrentUser { get; set; }
    }
}