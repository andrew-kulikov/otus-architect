using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Service.Listener.Builders;
using FeedHistory.Service.Listener.Cache;
using FeedHistory.Service.Listener.Storage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly ICacheInitializer _cacheInitializer;

        public Worker(ILogger<Worker> logger, IBarsRepository barsRepository, IDbInitializer dbInitializer, IConfiguration configuration, ICacheInitializer cacheInitializer)
        {
            _logger = logger;
            _barsRepository = barsRepository;
            _dbInitializer = dbInitializer;
            _configuration = configuration;
            _cacheInitializer = cacheInitializer;
            _barsBuilder = new BarsBuilder(_barsRepository);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _dbInitializer.InitializeAsync();
            await _cacheInitializer.InitializeAsync(stoppingToken);

            var connection = new HubConnectionBuilder()
                .WithUrl(_configuration.GetValue<string>("Feed:Url"))
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