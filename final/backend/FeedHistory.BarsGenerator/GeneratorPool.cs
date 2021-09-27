using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.BarsGenerator.Exporters;
using FeedHistory.BarsGenerator.Generators;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator
{
    public class GeneratorPool
    {
        private readonly string _symbol;
        private readonly int _batchSize;
        private readonly BarsBatchGenerator[] _generators;
        private readonly IExporter _exporter;

        public GeneratorPool(string symbol, IExporter exporter)
        {
            _symbol = symbol;
            _exporter = exporter;
            _batchSize = 10000;

            _generators = Enum.GetValues<BarPeriod>()
                .Select(period => new BarsBatchGenerator(
                    period, symbol, _batchSize, 
                    new DateTime(2021, 6, 1).ToTimestamp(), 
                    new DateTime(2021, 9, 26).ToTimestamp()))
                .ToArray();
        }

        public void Run()
        {
            var generatorTasks = _generators.Select(generator => Task.Run(async () => await RunGeneratorAsync(generator))).ToArray();

            Task.WaitAll(generatorTasks);
        }

        private async Task RunGeneratorAsync(BarsBatchGenerator generator)
        {
            Console.WriteLine($"Starting generator for period {generator.Period}");

            var sw = new Stopwatch();
            sw.Start();

            foreach (var batch in generator)
            {
                if (ShouldReport(batch.BatchId, generator.Period))
                {
                    Console.WriteLine($"Generator {_symbol}_{generator.Period} created batch #{batch.BatchId}");
                }

                await _exporter.ExportBatchAsync(batch);
            }

            sw.Stop();

            Console.WriteLine($"Generator {_symbol}_{generator.Period} completed. Total time: {sw.Elapsed}");
            Console.WriteLine();
            Console.WriteLine();
        }

        private bool ShouldReport(int batchId, BarPeriod period)
        {
            var normalizedId = 50000 / _batchSize / period.GetEstimatedMinutes();

            return normalizedId == 0 || batchId % normalizedId == 0;
        }
    }
}