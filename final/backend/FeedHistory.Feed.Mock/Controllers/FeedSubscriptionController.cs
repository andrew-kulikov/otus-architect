using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FeedHistory.Feed.Mock.Generators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FeedHistory.Feed.Mock.Controllers
{
    [Route("feed")]
    public class FeedSubscriptionController : ControllerBase
    {
        [HttpGet]
        [Route("subscribe")]
        public async Task Subscribe()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            TickGenerator generator = null;

            var resultString = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, result.Count));
            if (resultString.StartsWith("sub "))
            {
                var symbols = resultString.Substring(4).Split(";").Select(s => s.Trim()).ToList();

                Console.WriteLine($"Subsribed to symbols: {string.Join(';', symbols)}");
                
                generator = new TickGenerator(symbols);

                generator.Tick += async tick =>
                {
                    var serializedTick = JsonSerializer.SerializeToUtf8Bytes(tick);
                    await webSocket.SendAsync(serializedTick, WebSocketMessageType.Text, true, CancellationToken.None);
                };
                generator.Start();
            }

            while (!result.CloseStatus.HasValue)
            {
                //await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            Console.WriteLine("Stopping...");

            generator?.Stop();
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}