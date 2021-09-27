using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task InitializeAsync();
    }

    public class CacheInitializer : ICacheInitializer, IDisposable
    {
        private Box _tarantoolClient;
        private readonly IConfiguration _configuration;
        private readonly IBarsRepository _barsRepository;

        public CacheInitializer(IConfiguration configuration, IBarsRepository barsRepository)
        {
            _configuration = configuration;
            _barsRepository = barsRepository;
        }

        public async Task InitializeAsync()
        {
            _tarantoolClient = await Box.Connect("localhost", 3302);

            await CreateSchemaAsync();

            foreach (var symbolId in Enumerable.Range(1, 1000))
            {
                var symbolName = $"S{symbolId}";

                foreach (var barPeriod in Enum.GetValues<BarPeriod>())
                {
                    var to = DateTime.UtcNow;
                    var from = GetPeriodStartInterval(barPeriod, to);

                    var bars = await _barsRepository.GetBarsAsync(symbolName, barPeriod, from.ToTimestampMilliseconds(), to.ToTimestampMilliseconds());

                    await SaveToTarantoolAsync(bars, symbolName, barPeriod);

                    Console.WriteLine($"Saved symbol {symbolName}_{barPeriod} to cache");
                }
            }
        }

        private async Task SaveToTarantoolAsync(ICollection<Bar> bars, string symbol, BarPeriod period)
        {
            var index = _tarantoolClient.Schema["bars"]["primary"];
            foreach (var bar in bars)
            {
                try
                {
                    await index.Insert(bar.ToTuple(period, symbol));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                   // throw;
                }
               
            }
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

        public async Task CreateSchemaAsync()
        {
            await _tarantoolClient.Eval<string>($"box.schema.create_space('bars', {{if_not_exists = true}})");

            // symbol, period, time
            var primaryParts = "{{field = 1, type = 'string'}, {field = 2, type = 'unsigned'}, {field = 3, type = 'unsigned'}}";

            await _tarantoolClient.Eval<string>(
                $"box.space.bars:create_index('primary', {{unique = true, if_not_exists = true, parts = {primaryParts}}})");

            await _tarantoolClient.Schema.Reload();
        }

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
