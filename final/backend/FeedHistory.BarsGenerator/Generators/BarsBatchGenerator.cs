using System.Collections;
using System.Collections.Generic;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Generators
{
    public class BarsBatchGenerator : IEnumerable<BarsBatch>
    {
        public BarsBatchGenerator(BarPeriod period, string symbol, int batchSize, long startTime, long endTime)
        {
            Period = period;
            Symbol = symbol;
            BatchSize = batchSize;
            EndTime = endTime;
            StartTime = startTime;
        }

        public BarPeriod Period { get; }
        public string Symbol { get; }
        public int BatchSize { get; }
        public long EndTime { get; }
        public long StartTime { get; }

        public IEnumerator<BarsBatch> GetEnumerator() =>
            new BarsBatchEnumerator(new BarsEnumerator(Period, Symbol, StartTime, EndTime), BatchSize);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}