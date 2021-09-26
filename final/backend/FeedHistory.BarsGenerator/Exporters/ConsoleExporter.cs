using System;
using System.Threading.Tasks;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Exporters
{
    public class ConsoleExporter: IExporter
    {
        public Task ExportBatchAsync(BarsBatch batch)
        {
            Console.WriteLine($"Batch #{batch.BatchId}");

            foreach (var batchBar in batch.Bars)
            {
                Console.WriteLine(batchBar);
            }

            return Task.CompletedTask;
        }
        
    }
}