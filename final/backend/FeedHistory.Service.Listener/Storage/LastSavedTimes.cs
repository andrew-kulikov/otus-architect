using System.Collections.Generic;

namespace FeedHistory.Service.Listener.Storage
{
    public class LastSavedTimes
    {
        public Dictionary<string, Dictionary<string, long>> SymbolTimes { get; set; }
    }
}