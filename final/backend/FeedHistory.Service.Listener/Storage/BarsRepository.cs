using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FeedHistory.Service.Listener.Storage
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }

    public class MongoDbInitializer : IDbInitializer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MongoDbInitializer> _logger;

        public MongoDbInitializer(IConfiguration configuration, ILogger<MongoDbInitializer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            var client = new MongoClient(_configuration.GetValue<string>("Mongo:ConnectionString"));

            var db = client.GetDatabase("bars");
            var symbolIndex = new IndexKeysDefinitionBuilder<MongoBar>().Ascending(b => b.S);
            var timeIndex = new IndexKeysDefinitionBuilder<MongoBar>().Ascending(b => b.S).Descending(b => b.T);

            foreach (var barPeriod in Enum.GetValues<BarPeriod>())
            {
                var collection = db.GetCollection<MongoBar>(barPeriod.ToString());
                
                await collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoBar>(symbolIndex, new CreateIndexOptions {Name = "symbol"}));
                await collection.Indexes.CreateOneAsync(new CreateIndexModel<MongoBar>(timeIndex, new CreateIndexOptions {Name = "symbol_time"}));

                _logger.LogInformation($"Initialized indexes for collection [{barPeriod}]");
            }
        }
    }

    public class BarsSnapshot
    {
        public long Time { get; set; }
        public List<SymbolBarsSnapshot> SymbolSnapshots { get; set; }
    }

    public class SymbolBarsSnapshot
    {
        public string Symbol { get; set; }
        public Dictionary<string, List<Bar>> PeriodBars { get; set; }
    }

    public class LastSavedTimes
    {
        public Dictionary<string, Dictionary<string, long>> SymbolTimes { get; set; }
    }

    public interface IBarsRepository
    {
        Task<LastSavedTimes> SaveSnapshotAsync(BarsSnapshot snapshot);
    }

    public class BarsRepository : IBarsRepository
    {
        private readonly IConfiguration _configuration;

        public BarsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
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

    public class MongoBar
    {
        [BsonId] public ObjectId Id { get; set; }

        public long T { get; set; }
        public string S { get; set; }
        public double O { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double C { get; set; }
        public double V { get; set; }
    }
}