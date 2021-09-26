using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FeedHistory.Common;
using FeedHistory.Common.Extensions;
using FeedHistory.Service.Api.Storage;
using Microsoft.AspNetCore.Mvc;

namespace FeedHistory.Service.Api.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly IBarsRepository _barsRepository;

        public HistoryController(IBarsRepository barsRepository)
        {
            _barsRepository = barsRepository;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(BarsResponse), 200)]
        public async Task<IActionResult> GetBars(
            [FromQuery] string symbol,
            [FromQuery] long from,
            [FromQuery] long to,
            [FromQuery] string resolution)
        {
            var period = UtilityExtensions.ResolveBarPeriod(resolution);
            var bars = await _barsRepository.GetBarsAsync(symbol, period, from, to);

            BarsResponse response = bars.Any() 
                ? SuccessBarsResponse.FromBars(bars) 
                : new NoDataBarsResponse();

            return Ok(response);
        }
    }


    public class BarsResponse
    {
        public BarsResponse(string status)
        {
            Status = status;
        }

        [JsonPropertyName("s")] public string Status { get; }
    }

    public class NoDataBarsResponse : BarsResponse
    {
        public NoDataBarsResponse() : base("no_data")
        {
        }
    }

    public class NextBarBarsResponse : BarsResponse
    {
        public NextBarBarsResponse() : base("no_data")
        {
        }

        [JsonPropertyName("nb")] public long NextBar { get; set; }
    }


    public class SuccessBarsResponse : BarsResponse
    {
        public SuccessBarsResponse() : base("ok")
        {
            Time = new List<long>();
            Open = new List<double>();
            High = new List<double>();
            Low = new List<double>();
            Close = new List<double>();
            Volume = new List<double>();
        }

        public List<long> Time { get; set; }
        public List<double> Open { get; set; }
        public List<double> High { get; set; }
        public List<double> Low { get; set; }
        public List<double> Close { get; set; }
        public List<double> Volume { get; set; }

        public static SuccessBarsResponse FromBars(ICollection<Bar> bars)
        {
            var result = new SuccessBarsResponse();

            foreach (var bar in bars)
            {
                result.Time.Add(bar.Time);
                result.Open.Add(bar.Open);
                result.High.Add(bar.High);
                result.Low.Add(bar.Low);
                result.Close.Add(bar.Close);
                result.Volume.Add(bar.Volume);
            }

            return result;
        }
    }
}