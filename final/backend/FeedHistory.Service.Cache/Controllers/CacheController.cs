using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeedHistory.Common;
using Microsoft.AspNetCore.Mvc;

namespace FeedHistory.Service.Cache.Controllers
{
    public interface IBarsRepository
    {
        void Save(ICollection<Bar> bars, string symbol, BarPeriod period);
        ICollection<Bar> GetBarsAsync(string symbol, BarPeriod period, long from, long to);
    }

    public class BarsRepository : IBarsRepository
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<BarPeriod, List<Bar>>> _cache;

        public BarsRepository()
        {
            _cache = new ConcurrentDictionary<string, ConcurrentDictionary<BarPeriod, List<Bar>>>();
        }

        public void Save(ICollection<Bar> bars, string symbol, BarPeriod period)
        {
            var symbolCache = _cache.GetOrAdd(symbol, _ => new ConcurrentDictionary<BarPeriod, List<Bar>>());
            var periodCache = symbolCache.GetOrAdd(period, _ => new List<Bar>());

            periodCache.AddRange(bars);
        }

        public ICollection<Bar> GetBarsAsync(string symbol, BarPeriod period, long from, long to)
        {
            if (!_cache.TryGetValue(symbol, out var symbolCache)) return new List<Bar>();
            if (!symbolCache.TryGetValue(period, out var bars)) return new List<Bar>();

            return bars.Where(b => b.Time >= from && b.Time <= to).ToList();
        }
    }

    [Route("api/cache")]
    [ApiController]
    public class CacheController: ControllerBase
    {
        private readonly IBarsRepository _barsRepository;

        public CacheController(IBarsRepository barsRepository)
        {
            _barsRepository = barsRepository;
        }

        public IActionResult Get(
            [FromQuery] string symbol,
            [FromQuery] BarPeriod period,
            [FromQuery] long from,
            [FromQuery] long to)
        {
            var bars = _barsRepository.GetBarsAsync(symbol, period, from, to);

            return new JsonResult(bars);
        }

        [HttpPost("")]
        public IActionResult SaveToCache(
            [FromQuery] string symbol,
            [FromQuery] BarPeriod period,
            [FromBody] ICollection<Bar> bars)
        {
            _barsRepository.Save(bars, symbol, period);

            return Ok();
        }
    }
}
