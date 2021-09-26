using System;
using System.Threading.Tasks;
using FeedHistory.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FeedHistory.Service.Listener.Storage
{
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
}