using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Infrastructure.HubBackplane;
using SocialNetwork.Web.Utils;

namespace SocialNetwork.Web.Hubs
{
    public class NewsFeedHub : Hub
    {
        private readonly INewsHubBackplane _newsHubBackplane;

        public NewsFeedHub(INewsHubBackplane newsHubBackplane)
        {
            _newsHubBackplane = newsHubBackplane;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var userId = Context.User.GetUserId();

                await Groups.AddToGroupAsync(Context.ConnectionId, $"feed-{userId}");

                await _newsHubBackplane.SubscribeToUserFeedAsync(userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                var userId = Context.User.GetUserId();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"feed-{userId}");

                await _newsHubBackplane.UnsubscribeFromUserFeedAsync(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}