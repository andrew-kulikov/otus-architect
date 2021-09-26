using System;
using System.Collections.Generic;
using System.Linq;
using FeedHistory.Common;
using FeedHistory.Common.Extensions;

namespace FeedHistory.Service.Listener.Builders
{
    public class PeriodBarsBuilder
    {
        private readonly BarPeriod _barPeriod;
        private readonly string _symbol;
        private List<Bar> _currentBars;

        public PeriodBarsBuilder(Bar currentBar, BarPeriod barPeriod, string symbol)
        {
            _barPeriod = barPeriod;
            _symbol = symbol;

            _currentBars = new List<Bar>();
            if (currentBar != null) _currentBars.Add(currentBar);
        }

        public PeriodBarsBuilder(BarPeriod barPeriod, string symbol) : this(null, barPeriod, symbol)
        {
        }

        public void Cleanup(long tillTime)
        {
            _currentBars = _currentBars.Where(b => b.Time > tillTime).ToList();
            Console.WriteLine($"{_symbol}_{_barPeriod}. Cleanup till {tillTime}");
        }

        public List<Bar> GetSnapshot() => _currentBars.Select(b => b.Copy()).ToList();

        public void Advance(Tick tick)
        {
            if (!_currentBars.Any())
            {
                CreateNewBar(tick);
                return;
            }

            AppendTick(tick);
        }

        private void AppendTick(Tick tick)
        {
            var currentBar = _currentBars.Last();
            if (IsTickInBar(currentBar, tick))
            {
                UpdateBar(currentBar, tick);
                return;
            }

            CreateNewBar(tick);
        }

        private void UpdateBar(Bar currentBar, Tick tick)
        {
            if (currentBar.High < tick.Bid) currentBar.High = tick.Bid;
            if (currentBar.Low > tick.Bid) currentBar.Low = tick.Bid;

            currentBar.Close = tick.Bid;
            currentBar.Volume += tick.Volume;

            //Console.WriteLine($"Period {_barPeriod}. Updated bar {currentBar}");
        }

        private void CreateNewBar(Tick tick)
        {
            var barStartTime = UtilityExtensions.FindBarStartMilliseconds(tick.Time.FromTimestampMilliseconds(), _barPeriod);
            var bar = new Bar
            {
                Time = barStartTime,
                Open = tick.Bid,
                Close = tick.Bid,
                High = tick.Bid,
                Low = tick.Bid,
                Volume = tick.Volume
            };

            _currentBars.Add(bar);

            Console.WriteLine($"{_symbol}_{_barPeriod}. Created new bar {bar}");
        }

        private bool IsTickInBar(Bar bar, Tick tick) =>
            UtilityExtensions.IsInInterval(
                bar.Time.FromTimestampMilliseconds(),
                _barPeriod,
                tick.Time.FromTimestampMilliseconds());
    }
}