using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Service.Listener.Storage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedHistory.Service.Listener
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly BarsBuilder _barsBuilder;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _barsBuilder = new BarsBuilder();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/feedHub")
                .WithAutomaticReconnect()
                .AddJsonProtocol()
                .Build();

            await connection.StartAsync(stoppingToken);

            await connection.InvokeAsync<List<string>>("Subscribe", new List<string> {"AAPL", "TSLA", "MS", "EURUSD"}, stoppingToken);

            connection.On<Tick>("tick", tick =>
            {
                //_logger.LogInformation(tick.ToString());
                _barsBuilder.Advance(tick);
            });
        }
    }
}