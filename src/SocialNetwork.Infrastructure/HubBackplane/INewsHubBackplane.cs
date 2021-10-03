using System.Threading.Tasks;
using SocialNetwork.Core.Entities;

namespace SocialNetwork.Infrastructure.HubBackplane
{
    public interface INewsHubBackplane
    {
        Task SubscribeToUserFeedAsync(long userId);
        Task UnsubscribeFromUserFeedAsync(long userId);
        Task PublishAsync(UserPost post, long targetUserId);
    }
}