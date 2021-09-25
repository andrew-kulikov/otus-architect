namespace FeedHistory.Common
{
    public class Tick
    {
        public long Time { get; set; }
        public string Symbol { get; set; }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public double Volume { get; set; }

        public override string ToString() => $"{Time}|{Symbol}|Ask:{Ask}|Bid:{Bid}|V:{Volume}";
    }
}