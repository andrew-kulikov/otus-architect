using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.Common;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FeedHistory.Service.Listener.Storage
{
    public class BarsRepository : IBarsRepository
    {
        private readonly IConfiguration _configuration;

        public BarsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ICollection<Bar>> GetBarsAsync(string symbol, BarPeriod period, long from, long to)
        {
            var client = new MongoClient(_configuration.GetValue<string>("Mongo:ConnectionString"));

            var db = client.GetDatabase("bars");
            var collection = db.GetCollection<MongoBar>(period.ToString());

            var result = await collection
                .Find(b => b.S == symbol && b.T >= from && b.T <= to)
                .SortByDescending(b => b.T)
                .ToListAsync();

            return result.Select(b => new Bar
            {
                Time = b.T,
                Open = b.O,
                High = b.H,
                Low = b.L,
                Close = b.C,
                Volume = b.V
            }).ToList();
        }

        public async Task<LastSavedTimes> SaveSnapshotAsync(BarsSnapshot snapshot)
        {
            var client = new MongoClient(_configuration.GetValue<string>("Mongo:ConnectionString"));

            var metadataDb = client.GetDatabase("metadata");
            var metadataCollection = metadataDb.GetCollection<BarsSnapshot>("snapshots");
            await metadataCollection.InsertOneAsync(snapshot);

            var barsDb = client.GetDatabase("bars");

            return await SaveBarsSnapshotAsync(barsDb, snapshot);
        }

        private async Task<LastSavedTimes> SaveBarsSnapshotAsync(IMongoDatabase barsDb, BarsSnapshot snapshot)
        {
            var result = new Dictionary<string, Dictionary<string, long>>();

            foreach (var symbolBarsSnapshot in snapshot.SymbolSnapshots)
            {
                var periodTasks = symbolBarsSnapshot.PeriodBars.Select(p => SavePeriodBarsAsync(barsDb, symbolBarsSnapshot.Symbol, p.Key, p.Value));

                var periodTimes = await Task.WhenAll(periodTasks);
                result.Add(symbolBarsSnapshot.Symbol, new Dictionary<string, long>(periodTimes));

                Console.WriteLine($"Saved symbol {symbolBarsSnapshot.Symbol}");
            }

            return new LastSavedTimes
            {
                SymbolTimes = result
            };
        }

        private async Task<KeyValuePair<string, long>> SavePeriodBarsAsync(IMongoDatabase barsDb, string symbol, string period, List<Bar> bars)
        {
            var collection = barsDb.GetCollection<MongoBar>(period);

            var lastBar = await collection.Find(b => b.S == symbol).SortByDescending(b => b.T).Limit(1).FirstOrDefaultAsync();
            var lastTime = lastBar?.T ?? 0;

            var mappedBars = bars.Where(b => b.Time > lastTime).Select(b => Map(b, symbol)).ToList();

            if (mappedBars.Any()) await collection.InsertManyAsync(mappedBars);

            return new KeyValuePair<string, long>(period, lastTime);
        }

        private static MongoBar Map(Bar bar, string symbol) => new MongoBar
        {
            T = bar.Time,
            S = symbol,
            V = bar.Volume,
            O = bar.Open,
            L = bar.Low,
            C = bar.Close,
            H = bar.High
        };
    }
}