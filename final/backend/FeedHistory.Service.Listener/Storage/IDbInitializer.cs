using System.Threading.Tasks;

namespace FeedHistory.Service.Listener.Storage
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}