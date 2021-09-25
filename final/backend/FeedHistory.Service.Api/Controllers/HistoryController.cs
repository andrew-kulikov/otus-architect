using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FeedHistory.Service.Api.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        [HttpGet("")]
        [ProducesResponseType(typeof(BarsResponse), 200)]
        public async Task<IActionResult> GetBars(
            [FromQuery] string symbol,
            [FromQuery] long from,
            [FromQuery] long to,
            [FromQuery] string resolution)
        {
            var response = new SuccessBarsResponse
            {
                
            };

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
        }

        public List<long> Time { get; set; }
        public List<double> Open { get; set; }
        public List<double> High { get; set; }
        public List<double> Low { get; set; }
        public List<double> Close { get; set; }
        public List<double> Volume { get; set; }
    }
}