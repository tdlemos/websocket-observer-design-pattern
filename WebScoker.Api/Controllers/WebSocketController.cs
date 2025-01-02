using Microsoft.AspNetCore.Mvc;
using Observer.Lib;
using System.Net.WebSockets;

namespace WebScoker.Api.Controllers;
public class WebSocketController(ISubject subject) : Controller
{
    private WebSocket? webSocket;
    private IObserver? _observer;

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using (webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                _observer = new SampleObserver(webSocket);
                subject.Attach(_observer);
                await Echo(webSocket);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        subject.Detach(_observer);

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
