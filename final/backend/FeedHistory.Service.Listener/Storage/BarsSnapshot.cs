using System.Collections.Generic;

namespace FeedHistory.Service.Listener.Storage
{
    public class BarsSnapshot
    {
        public long Time { get; set; }
        public List<SymbolBarsSnapshot> SymbolSnapshots { get; set; }
    }
}