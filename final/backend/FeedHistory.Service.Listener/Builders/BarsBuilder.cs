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
}