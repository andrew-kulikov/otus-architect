using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeedHistory.Service.Listener
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/feedHub")
                .WithAutomaticReconnect()
                //.ConfigureLogging(options =>
                //{
                //    options.ClearProviders();
                //    options.SetMinimumLevel(LogLevel.Trace);
                //    options.AddConsole();
                //})
                .AddJsonProtocol()
                .Build();

            await connection.StartAsync(stoppingToken);

            await connection.InvokeAsync<List<string>>("Subscribe", new List<string> {"AAPL"}, stoppingToken);

            connection.On<Tick>("tick", tick => _logger.LogInformation(tick.ToString()));
        }
    }

    public class Tick
    {
        public string Symbol { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double Volume { get; set; }

        public override string ToString() => $"{Symbol}|Ask:{Ask}|Bid:{Bid}|V:{Volume}";
    }
}