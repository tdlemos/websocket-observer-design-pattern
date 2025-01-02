using System.Net.WebSockets;
using System.Text;

namespace Observer.Lib;

public class SampleObserver : IObserver
{
    private WebSocket _webSocket { get; set; }

    public SampleObserver(WebSocket webSocket)
    {
        _webSocket = webSocket;
    }

    public async Task Update(string message)
    {
        var response = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(response);
        await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
