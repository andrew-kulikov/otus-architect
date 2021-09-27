using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Common.Extensions;
using FeedHistory.Service.Listener.Storage;
using Microsoft.Extensions.Configuration;
using ProGaudi.Tarantool.Client;
using ProGaudi.Tarantool.Client.Model;

namespace FeedHistory.Service.Listener.Cache
{
    public interface ICacheInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }

    public class CacheInitializer : ICacheInitializer, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IBarsRepository _barsRepository;

        public CacheInitializer(IHttpClientFactory httpClientFactory, IBarsRepository barsRepository)
        {
            _httpClient = httpClientFactory.CreateClient();
            _barsRepository = barsRepository;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            foreach (var symbolId in Enumerable.Range(1000, 1000))
            {
                var symbolName = $"S{symbolId}";

                foreach (var barPeriod in Enum.GetValues<BarPeriod>())
                {
                    // TODO: Remove
                    if (barPeriod == BarPeriod.M1 || barPeriod == BarPeriod.M5 || barPeriod == BarPeriod.M15 || barPeriod == BarPeriod.M30) continue;
                    
                    var to = DateTime.UtcNow;
                    var from = GetPeriodStartInterval(barPeriod, to);

                    var bars = await _barsRepository.GetBarsAsync(symbolName, barPeriod, from.ToTimestampMilliseconds(), to.ToTimestampMilliseconds());

                    if (bars.Any())
                    {
                        await SaveAsync(bars, symbolName, barPeriod, cancellationToken);

                        Console.WriteLine($"Saved symbol {symbolName}_{barPeriod} to cache");
                    }
                }
            }
        }

        private async Task SaveAsync(ICollection<Bar> bars, string symbol, BarPeriod period, CancellationToken cancellationToken)
        {
            var serializedBars = JsonSerializer.Serialize(bars);
            var content = new StringContent(serializedBars, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync($"http://localhost:7012/api/cache?symbol={symbol}&period={period}", content, cancellationToken);
        }

        private DateTime GetPeriodStartInterval(BarPeriod period, DateTime to) =>
            period switch
            {
                BarPeriod.M1 => to.AddDays(-5),
                BarPeriod.M5 => to.AddDays(-5 * 5),
                BarPeriod.M15 => to.AddDays(-5 * 15),
                BarPeriod.M30 => to.AddDays(-5 * 30),
                BarPeriod.H1 => to.AddDays(-5 * 60),
                BarPeriod.H4 => to.AddDays(-5 * 240),
                BarPeriod.D1 => to.AddYears(-20),
                BarPeriod.W1 => to.AddYears(-50),
                BarPeriod.Mo1 => to.AddYears(-50),
                _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
            };

        public void Dispose()
        {
        }
    }

    public static class TarantoolExtensions
    {
        public static TarantoolTuple<string, int, long, double, double, double, double, double> ToTuple(this Bar bar, BarPeriod period, string symbol) =>
            new TarantoolTuple<string, int, long, double, double, double, double, double>(symbol, (int)period, bar.Time, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
        
        public static Bar ToBar(this TarantoolTuple<string, int, long, double, double, double, double, double> barTuple) =>
            new Bar
            {
                Time = barTuple.Item3,
                Open = barTuple.Item4,
                High = barTuple.Item5,
                Low = barTuple.Item6,
                Close = barTuple.Item7,
                Volume = barTuple.Item8
            };
    }
}
