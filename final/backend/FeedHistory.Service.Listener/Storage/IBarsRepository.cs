using System.Threading.Tasks;

namespace FeedHistory.Service.Listener.Storage
{
    public interface IBarsRepository
    {
        Task<LastSavedTimes> SaveSnapshotAsync(BarsSnapshot snapshot);
    }
}