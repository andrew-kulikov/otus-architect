using System.Threading.Tasks;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator.Exporters
{
    public interface IExporter
    {
        Task ExportBatchAsync(BarsBatch batch);
    }
}