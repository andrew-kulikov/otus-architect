using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FeedHistory.Feed.Mock.Hubs
{
    public class FeedHub : Hub
    {
        private static readonly ConcurrentDictionary<string, List<string>> ConnectionSymbols = new ConcurrentDictionary<string, List<string>>();

        public async Task Subscribe(List<string> symbols)
        {
            foreach (var symbol in symbols) await Groups.AddToGroupAsync(Context.ConnectionId, symbol);

            ConnectionSymbols.TryAdd(Context.ConnectionId, symbols);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionSymbols.TryGetValue(Context.ConnectionId, out var symbols))
            {
                foreach (var symbol in symbols) await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);

                ConnectionSymbols.TryRemove(Context.ConnectionId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}