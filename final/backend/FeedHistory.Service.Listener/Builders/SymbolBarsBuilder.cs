using System;
using System.Collections.Generic;
using System.Linq;
using FeedHistory.Common;
using FeedHistory.Service.Listener.Storage;

namespace FeedHistory.Service.Listener.Builders
{
    public class SymbolBarsBuilder
    {
        private readonly string _symbol;
        private readonly Dictionary<BarPeriod, PeriodBarsBuilder> _periodBuilders;

        public SymbolBarsBuilder(string symbol)
        {
            _symbol = symbol;

            _periodBuilders = new Dictionary<BarPeriod, PeriodBarsBuilder>();
        }

        public void Initialize()
        {
            foreach (var period in Enum.GetValues<BarPeriod>())
            {
                _periodBuilders.Add(period, new PeriodBarsBuilder(period, _symbol));
            }
        }

        public SymbolBarsSnapshot GetSnapshot()
        {
            var periodBars = _periodBuilders.ToDictionary(p => p.Key.ToString(), p => p.Value.GetSnapshot());
            
            return new SymbolBarsSnapshot
            {
                Symbol = _symbol,
                PeriodBars = periodBars
            };
        }

        public void Cleanup(Dictionary<string, long> periodTimes)
        {
            foreach (var periodBarsBuilder in _periodBuilders)
            {
                if (periodTimes.TryGetValue(periodBarsBuilder.Key.ToString(), out var cleanupTime))
                {
                    periodBarsBuilder.Value.Cleanup(cleanupTime);
                }
            }
        }

        public void Advance(Tick tick)
        {
            foreach (var periodBarsBuilder in _periodBuilders)
            {
                periodBarsBuilder.Value.Advance(tick);
            }
        }
    }
}