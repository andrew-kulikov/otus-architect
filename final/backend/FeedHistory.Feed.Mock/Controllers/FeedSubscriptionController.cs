using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FeedHistory.Feed.Mock.Controllers
{
    [Route("feed")]
    public class FeedSubscriptionController : ControllerBase
    {
        [HttpGet]
        [Route("subscribe")]
        public async Task Subscribe()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            TickGenerator generator = null;

            var resultString = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, result.Count));
            if (resultString.StartsWith("sub "))
            {
                var symbols = resultString.Substring(4).Split(";").Select(s => s.Trim()).ToList();

                Console.WriteLine($"Subsribed to symbols: {string.Join(';', symbols)}");
                
                generator = new TickGenerator(symbols);

                generator.Tick += async tick =>
                {
                    var serializedTick = JsonSerializer.SerializeToUtf8Bytes(tick);
                    await webSocket.SendAsync(serializedTick, WebSocketMessageType.Text, true, CancellationToken.None);
                };
                generator.Start();
            }

            while (!result.CloseStatus.HasValue)
            {
                //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            Console.WriteLine("Stopping...");

            generator?.Stop();
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

    public class TickGenerator
    {
        private readonly List<string> _symbols;
        private List<SymbolTickGenerator> _generators;

        public TickGenerator(List<string> symbols)
        {
            _symbols = symbols;

        }

        public event Action<Tick> Tick;

        public void Start()
        {
            var random = new Random(42);
            
            _generators = _symbols.Select(symbol => new SymbolTickGenerator(symbol, random, 100)).ToList();

            foreach (var generator in _generators)
            {
                generator.Tick += Tick;
                generator.Start();
            }
        }

        public void Stop()
        {
            foreach (var generator in _generators)
            {
                generator.Stop();
                generator.Tick -= Tick;
            }
        }
    }

    public class SymbolTickGenerator
    {
        private readonly string _symbol;
        private readonly Random _random;
        private readonly int _intervalMilliseconds;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public SymbolTickGenerator(string symbol, Random random, int intervalMilliseconds)
        {
            _symbol = symbol;
            _random = random;
            _intervalMilliseconds = intervalMilliseconds;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public event Action<Tick> Tick;

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var tick = new Tick
                    {
                        Ask = _random.NextDouble(),
                        Bid = _random.NextDouble(),
                        Symbol = _symbol,
                        Volume = _random.NextDouble()
                    };

                    Tick?.Invoke(tick);

                    await Task.Delay(_intervalMilliseconds);
                }
            });
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public class Tick
    {
        public string Symbol { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double Volume { get; set; }
    }
}