using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.BarsGenerator.Models;
using MessagePack;

namespace FeedHistory.BarsGenerator.Exporters
{
    public class LocalMessagePackExporter : IExporter
    {
        private const string DatabasePath = @"C:\Work\feed-history-storage\data";
        private static readonly MessagePackSerializerOptions Lz4Options = 
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        public async Task ExportBatchAsync(BarsBatch batch)
        {
            var targetDirectory = Path.Join(DatabasePath, batch.Symbol, batch.Period.ToString());
            if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

            await WriteBars(batch, BarType.Ask, targetDirectory);
            await WriteBars(batch, BarType.Bid, targetDirectory);
        }

        private async Task WriteBars(BarsBatch batch, BarType barType, string targetDirectory)
        {
            var periodDirectory = Path.Join(targetDirectory, barType.ToString());
            if (!Directory.Exists(periodDirectory)) Directory.CreateDirectory(periodDirectory);

            var bars = batch.Bars.Where(b => b.Type == barType).Select(MessagePackedBar.FromBar).ToList();
            var chunkFile = Path.Join(periodDirectory, $"{bars[0].Time}_{bars[^1].Time}");

            await using var fileStream = new FileStream(chunkFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            await MessagePackSerializer.SerializeAsync(fileStream, bars, Lz4Options);
        }
    }

    [MessagePackObject]
    public class MessagePackedBar
    {
        [Key(0)] public long Time { get; set; }
        [Key(1)] public double O { get; set; }
        [Key(2)] public double H { get; set; }
        [Key(3)] public double L { get; set; }
        [Key(4)] public double C { get; set; }
        [Key(5)] public double V { get; set; }

        public static MessagePackedBar FromBar(Bar bar) =>
            new()
            {
                Time = bar.Time,
                C = bar.C,
                H = bar.H,
                L = bar.L,
                O = bar.O,
                V = bar.V
            };
    }
}