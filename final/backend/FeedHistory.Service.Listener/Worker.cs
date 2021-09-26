using System.Collections.Generic;
using System.Linq;
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
        private readonly IBarsRepository _barsRepository;
        private readonly IDbInitializer _dbInitializer;

        public Worker(ILogger<Worker> logger, IBarsRepository barsRepository, IDbInitializer dbInitializer)
        {
            _logger = logger;
            _barsRepository = barsRepository;
            _dbInitializer = dbInitializer;
            _barsBuilder = new BarsBuilder(_barsRepository);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _dbInitializer.InitializeAsync();

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/feedHub")
                .WithAutomaticReconnect()
                .AddJsonProtocol()
                .Build();

            await connection.StartAsync(stoppingToken);

            var symbols = Enumerable.Range(1, 1000).Select(i => $"S{i}").ToList();
            await connection.InvokeAsync<List<string>>("Subscribe", symbols, stoppingToken);

            connection.On<Tick>("tick", tick =>
            {
                _barsBuilder.Advance(tick);
            });
        }
    }
}