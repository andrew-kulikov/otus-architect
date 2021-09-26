using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.Common;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FeedHistory.Service.Api.Storage
{
    public interface IBarsRepository
    {
        Task<ICollection<Bar>> GetBarsAsync(string symbol, BarPeriod period, long from, long to);
    }

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