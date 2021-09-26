using System;
using System.Collections;
using System.Collections.Generic;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Generators
{
    public class BarsEnumerator: IEnumerator<Bar>
    {
        public long StartTime { get; }
        private readonly Random _random = new Random(42);
        public BarPeriod Period { get; }
        public string Symbol { get; }
        public long EndTime { get; }

        public BarsEnumerator(BarPeriod period, string symbol, long startTime, long endTime)
        {
            Period = period;
            Symbol = symbol;
            EndTime = endTime;
            StartTime = startTime;
        }

        public bool MoveNext()
        {
            if (Current == null)
            {
                Current = BuildBar(BarType.Bid, StartTime);
                return true;
            }

            if (Current.Time >= EndTime) return false;

            if (Current.Type == BarType.Bid)
            {
                Current = BuildBar(BarType.Ask, Current.Time);
                return true;
            }

            Current = BuildBar(BarType.Bid, Current.Time.GetNextDate(Period));
            return true;
        }

        private Bar BuildBar(BarType type, long time)
        {
            return new Bar
            {
                Time = time,
                Period = Period,
                Symbol = Symbol,
                Type = type,
                C = _random.NextDouble() * 200 + 150,
                O = _random.NextDouble() * 200 + 150,
                H = _random.NextDouble() * 200 + 150,
                L = _random.NextDouble() * 200 + 150,
                V = _random.NextDouble() * 5,
            };
        }

        public void Reset()
        {
            Current = null;
        }

        public Bar Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {

        }
    }
}