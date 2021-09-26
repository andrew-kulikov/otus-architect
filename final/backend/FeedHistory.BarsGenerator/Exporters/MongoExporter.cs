using System.Linq;
using System.Threading.Tasks;
using FeedHistory.BarsGenerator.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FeedHistory.BarsGenerator.Exporters
{
    public class MongoExporter : IExporter
    {
        private readonly MongoClient _client;

        public MongoExporter()
        {
            _client = new MongoClient("mongodb://localhost:27018");
        }

        public async Task ExportBatchAsync(BarsBatch batch)
        {
            var database = _client.GetDatabase("bars");
            var collection = database.GetCollection<MongoBar>(batch.Period.ToString());

            using (var session = await _client.StartSessionAsync())
            {
                var docs = batch.Bars.Select(ToDocument);

                await collection.InsertManyAsync(docs);
            }
        }

        private MongoBar ToDocument(Bar bar) =>
            new()
            {
                T = bar.Time,
                S = bar.Symbol,
                O = bar.O,
                H = bar.H,
                L = bar.L,
                C = bar.L,
                V = bar.V
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