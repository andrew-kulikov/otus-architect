namespace FeedHistory.Common
{
    public class Bar
    {
        public long Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public override string ToString() => $"{Time}|O:{Open}|H:{High}|L:{Low}|C:{Close}";
    }

    public enum BarPeriod
    {
        M1,
        M5,
        M15,
        M30,
        H1,
        H4,
        D1,
        W1,
        Mo1
    }
}