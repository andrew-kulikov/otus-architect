using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace FeedHistory.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public HistoryController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetBars(
            [FromQuery] string symbol,
            [FromQuery] long from,
            [FromQuery] long to,
            [FromQuery] string resolution)
        {
            var symbolIdSource = symbol.Substring(1);
            if (!int.TryParse(symbolIdSource, out var symbolId)) return BadRequest("Invalid symbol name");

            var configs = _configuration.GetSection("Services").Get<ICollection<ServiceConnectionConfig>>();
            var matchingService = configs.FirstOrDefault(c => c.SymbolFrom <= symbolId && c.SymbolTo >= symbolId);
            if (matchingService == null) return NotFound();

            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"{matchingService.Url}/api/history?symbol={symbol}&from={from}&to={to}&resolution={resolution}");

            HttpContext.Response.RegisterForDispose(response);

            return new HttpResponseMessageResult(response);
        }
    }

    public class HttpResponseMessageResult : IActionResult
    {
        private readonly HttpResponseMessage _responseMessage;

        public HttpResponseMessageResult(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int) _responseMessage.StatusCode;

            context.HttpContext.Response.Headers.TryAdd("Content-Type", new StringValues("application/json"));

            using (var stream = await _responseMessage.Content.ReadAsStreamAsync())
            {
                await stream.CopyToAsync(context.HttpContext.Response.Body);
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }

    public class ServiceConnectionConfig
    {
        public string Url { get; set; }
        public int SymbolFrom { get; set; }
        public int SymbolTo { get; set; }
    }
}