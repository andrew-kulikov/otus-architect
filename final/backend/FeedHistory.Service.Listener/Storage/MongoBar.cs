using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeedHistory.Service.Listener.Storage
{
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