using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Common.Extensions;
using FeedHistory.Service.Listener.Storage;
using Timer = System.Timers.Timer;

namespace FeedHistory.Service.Listener.Builders
{
    public class BarsBuilder: IDisposable
    {
        private readonly ConcurrentQueue<Tick> _pendingTicks;
        private readonly Thread _tickUpdateThread;
        private readonly Timer _reportTimer;
        private readonly ConcurrentDictionary<string, SymbolBarsBuilder> _barsBuilders;
        private readonly IBarsRepository _barsRepository;
        private Dictionary<string, Dictionary<string, long>> _pendingCleanup = null;
        private readonly object _cleanupLock = new object();

        public BarsBuilder(IBarsRepository barsRepository)
        {
            _barsRepository = barsRepository;

            _pendingTicks = new ConcurrentQueue<Tick>();
            _barsBuilders = new ConcurrentDictionary<string, SymbolBarsBuilder>();
          
            _tickUpdateThread = new Thread(RunTickThread);
            _tickUpdateThread.Start();

            _reportTimer = new Timer(60_000);
            _reportTimer.Elapsed += async (e, a) => await BuildAndSaveSnapshotAsync();
            _reportTimer.Enabled = true;
        }

        private void RunTickThread()
        {
            while (true)
            {
                if (_pendingCleanup != null) Cleanup();

                ProcessTicks();

                Thread.Sleep(50);
            }
        }

        private void ProcessTicks()
        {
            var chunk = _pendingTicks.Flush();
            if (!chunk.Any()) return;
            
            foreach (var symbolTicks in chunk.GroupBy(c => c.Symbol))
            {
                var symbolBarsBuilder = _barsBuilders.GetOrAdd(symbolTicks.Key, symbol =>
                {
                    var builder = new SymbolBarsBuilder(symbol);
                    builder.Initialize();
                    return builder;
                });

                //Console.WriteLine($"Processing chunk for symbol {symbolTicks.Key}. Chunk size: {symbolTicks.Count()}");

                foreach (var tick in symbolTicks) symbolBarsBuilder.Advance(tick);
            }
        }

        private void Cleanup()
        {
            lock (_cleanupLock)
            {
                var copy = _pendingCleanup.ToDictionary(p => p.Key, p => p.Value);
                _pendingCleanup = null;

                foreach (var symbolCleanup in copy)
                {
                    if (_barsBuilders.TryGetValue(symbolCleanup.Key, out var builder))
                    {
                        builder.Cleanup(symbolCleanup.Value);
                    }
                }
            }
        }

        private async Task BuildAndSaveSnapshotAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                var symbolSnapshots = _barsBuilders.Select(b => b.Value.GetSnapshot()).ToList();

                var snapshot = new BarsSnapshot
                {
                    Time = DateTime.UtcNow.ToTimestampMilliseconds(),
                    SymbolSnapshots = symbolSnapshots
                };

                var result = await _barsRepository.SaveSnapshotAsync(snapshot);

                Cleanup(result.SymbolTimes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            sw.Stop();
            Console.WriteLine($"Saving report took {sw.Elapsed}");
        }

        public void Cleanup(Dictionary<string, Dictionary<string, long>> symbolTimes)
        {
            lock (_cleanupLock)
            {
                _pendingCleanup = symbolTimes;
            }
        }

        public void Advance(Tick tick)
        {
            _pendingTicks.Enqueue(tick);
        }

        public void Dispose()
        {
            _tickUpdateThread.Interrupt();
            _reportTimer.Enabled = false;
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