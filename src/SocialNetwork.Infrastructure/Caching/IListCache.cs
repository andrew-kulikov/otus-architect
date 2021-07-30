using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetwork.Infrastructure.Caching
{
    public interface IListCache<T>
    {
        Task<List<T>> GetAsync(string key);
        Task SetAsync(string key, List<T> data);
    }
}