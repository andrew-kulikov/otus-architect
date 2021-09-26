using System.Collections.Generic;

namespace FeedHistory.BarsGenerator.Models
{
    public class BarsBatch
    {
        public BarsBatch(int batchSize, string symbol, BarPeriod period)
        {
            BatchSize = batchSize;
            Period = period;
            Symbol = symbol;
            Bars = new List<Bar>(BatchSize);
        }

        public int BatchId { get; private set; }
        public string Symbol { get; set; }
        public ICollection<Bar> Bars { get; }
        public int BatchSize { get; }
        public BarPeriod Period { get; }

        public void Add(Bar bar)
        {
            Bars.Add(bar);
        }

        public void BeginNewBatch()
        {
            Bars.Clear();
            BatchId++;
        }
    }
}