using System.Collections;
using System.Collections.Generic;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Generators
{
    public class BarsGenerator : IEnumerable<Bar>
    {
        private readonly BarPeriod _period;
        private readonly string _symbol;
        private readonly long _startTime;
        private readonly long _endTime;

        public BarsGenerator(BarPeriod period, string symbol, long endTime, long startTime)
        {
            _period = period;
            _symbol = symbol;
            _endTime = endTime;
            _startTime = startTime;
        }

        public IEnumerator<Bar> GetEnumerator() => new BarsEnumerator(_period, _symbol, _startTime, _endTime);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}