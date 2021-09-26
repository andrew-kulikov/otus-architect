using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FeedHistory.Common.Extensions
{
    public static class LinqExtensions
    {
        public static List<T> Flush<T>(this ConcurrentQueue<T> queue)
        {
            var result = new List<T>();

            while (!queue.IsEmpty)
                if (queue.TryDequeue(out var item))
                    result.Add(item);

            return result;
        }
    }
}