using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Feed.Mock.Generators;
using FeedHistory.Feed.Mock.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedHistory.Feed.Mock.HostedServices
{
    public class TickSenderService : IHostedService
    {
        private readonly IHubContext<FeedHub> _hubContext;
        private readonly TickGenerator _tickGenerator;
        private readonly ILogger<TickSenderService> _logger;

        public TickSenderService(IHubContext<FeedHub> hubContext, ILogger<TickSenderService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            var symbols = Enumerable.Range(1, 1000).Select(i => $"S{i}").ToList();
            _tickGenerator = new TickGenerator(symbols);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting tick sender service...");

            var tickQueue = new ConcurrentQueue<Tick>();
            var tickSendTask = Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    while (tickQueue.TryDequeue(out var tick))
                    {
                        await _hubContext.Clients.Group(tick.Symbol).SendAsync("tick", tick, cancellationToken);
                    }

                    await Task.Delay(10, cancellationToken);
                }
            }, cancellationToken);

            _tickGenerator.Tick += tick => tickQueue.Enqueue(tick);
            _tickGenerator.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping tick sender service");

            _tickGenerator.Stop();

            return Task.CompletedTask;
        }
    }
}