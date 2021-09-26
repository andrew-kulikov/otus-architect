namespace FeedHistory.BarsGenerator.Models
{
    public class Bar
    {
        public long Time { get; set; }
        public string Symbol { get; set; }
        public BarPeriod Period { get; set; }
        public BarType Type { get; set; }
        public double O { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double C { get; set; }
        public double V { get; set; }

        public override string ToString() => $"{Symbol}|{Period}|{Type}|{Time} O = {O}, H = {H}, L = {L}, C = {C}";
    }
}