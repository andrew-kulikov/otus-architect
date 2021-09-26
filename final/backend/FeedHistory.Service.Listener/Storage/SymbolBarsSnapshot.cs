using System.Collections.Generic;
using FeedHistory.Common;

namespace FeedHistory.Service.Listener.Storage
{
    public class SymbolBarsSnapshot
    {
        public string Symbol { get; set; }
        public Dictionary<string, List<Bar>> PeriodBars { get; set; }
    }
}