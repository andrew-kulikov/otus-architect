using System;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Common.Extensions;

namespace FeedHistory.Feed.Mock.Generators
{
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
                        Volume = _random.NextDouble(),
                        Time = DateTime.UtcNow.ToTimestampMilliseconds()
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
}