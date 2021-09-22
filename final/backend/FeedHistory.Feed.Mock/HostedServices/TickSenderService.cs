using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            _tickGenerator = new TickGenerator(new List<string> {"AAPL", "TSLA", "MS", "EURUSD"});
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting tick sender service...");

            _tickGenerator.Tick += tick => { _hubContext.Clients.Group(tick.Symbol).SendAsync("tick", tick, cancellationToken); };

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