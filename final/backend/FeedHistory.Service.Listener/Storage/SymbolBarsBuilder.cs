using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FeedHistory.Common;

namespace FeedHistory.Service.Listener.Storage
{
    public class BarsBuilder: IDisposable
    {
        private readonly ConcurrentQueue<Tick> _pendingTicks;
        private readonly Thread _tickUpdateThread;
        private readonly ConcurrentDictionary<string, SymbolBarsBuilder> _barsBuilders;

        public BarsBuilder()
        {
            _pendingTicks = new ConcurrentQueue<Tick>();
            _barsBuilders = new ConcurrentDictionary<string, SymbolBarsBuilder>();
            _tickUpdateThread = new Thread(RunTickThread);
            _tickUpdateThread.Start();
        }

        private void RunTickThread()
        {
            while (true)
            {
                var chunk = Flush(_pendingTicks);
                if (chunk.Any())
                {
                    foreach (var symbolTicks in chunk.GroupBy(c => c.Symbol))
                    {
                        var symbolBarsBuilder = _barsBuilders.GetOrAdd(symbolTicks.Key, symbol =>
                        {
                            var builder = new SymbolBarsBuilder(symbol);

                            builder.Initialize();

                            return builder;
                        });

                        Console.WriteLine($"Processing chunk for symbol {symbolTicks.Key}. Chunk size: {symbolTicks.Count()}");

                        foreach (var tick in symbolTicks)
                        {
                            symbolBarsBuilder.Advance(tick);
                        }
                    }
                }

                Thread.Sleep(50);
            }
        }

        private List<Tick> Flush(ConcurrentQueue<Tick> ticks)
        {
            var result = new List<Tick>();

            while (!ticks.IsEmpty)
            {
                if (ticks.TryDequeue(out var tick)) result.Add(tick);    
            }

            return result;
        }

        public void Advance(Tick tick)
        {
            _pendingTicks.Enqueue(tick);
        }

        public void Dispose()
        {
            _tickUpdateThread.Interrupt();
        }
    }

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
                _periodBuilders.Add(period, new PeriodBarsBuilder(period));
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

    public class PeriodBarsBuilder
    {
        private readonly BarPeriod _barPeriod;
        private readonly List<Bar> _currentBars;

        public PeriodBarsBuilder(Bar currentBar, BarPeriod barPeriod)
        {
            _barPeriod = barPeriod;

            _currentBars = new List<Bar>();
            if (currentBar != null) _currentBars.Add(currentBar);
        }

        public PeriodBarsBuilder(BarPeriod barPeriod) : this(null, barPeriod)
        {
        }

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

            Console.WriteLine($"Period {_barPeriod}. Created new bar {bar}");
        }

        private bool IsTickInBar(Bar bar, Tick tick) =>
            UtilityExtensions.IsInInterval(
                bar.Time.FromTimestampMilliseconds(),
                _barPeriod,
                tick.Time.FromTimestampMilliseconds());
    }
}