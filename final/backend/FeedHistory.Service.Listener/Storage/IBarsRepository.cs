using System.Collections.Generic;
using System.Threading.Tasks;
using FeedHistory.Common;

namespace FeedHistory.Service.Listener.Storage
{
    public interface IBarsRepository
    {
        Task<LastSavedTimes> SaveSnapshotAsync(BarsSnapshot snapshot);
        Task<ICollection<Bar>> GetBarsAsync(string symbol, BarPeriod period, long from, long to);
    }
}