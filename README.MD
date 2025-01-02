# .NET 8 Observer Design Pattern and WebSocket

In this solution, I'm going to create a real-time notification using WebSocket and Observer design pattern.

The objective of this solution is to notify all clients connect via WebSocket with the current date and time.

To achieve the goal I created one project **Observer.Lib** containing Observer Design Pattern necessary files, one **WebApi** project to accept clients and one **Background service** to notify all clients.

## Observer - Design Pattern

Observer is a behavioral deisgn pattern that let you define a subscription mechanism to notify multiple objects about events that happen to the object they're observering.

This pattern consist in creating two classes: Observer and Subject.

Everytime a connection is established, one Observer class is create and added to Subject class that hold it in an array.

![](Images/solution1-en.png)

Everytime the Notification method of the Subject class is called it notifies all subscribed Observer class.

![](Images/solution2-en.png)

## Observer classes

**__Observer.Lib/ISubject.cs__** - ISubject Interface

```CSharp
namespace Observer.Lib;

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify(string message);
}
```

**__Observer.Lib/IObserver.cs__** - IObserver Interface

```CSharp
namespace Observer.Lib;

public interface IObserver
{
    Task Update(string message);
}
```

**__Observer.Lib/ISubject.cs__** - SampleSubject class implementing ISubject Interface

In this class I implemented the singleton pattern to garantee this class is always singleton. It's necessary that this class is singleton.

```CSharp
namespace Observer.Lib;

public class SampleSubject : ISubject
{
    private static SampleSubject? instance = null;
    private List<IObserver> _observers = [];
    
    private SampleSubject() { }

    public static SampleSubject Instance
    {
        get
        {
            // Check if the Instance already exists
            if (instance == null)
                instance = new SampleSubject();

            return instance;
        }
    }

    public void Attach(IObserver observer)
    {
        _observers.Add(observer);
    }
    public void Detach(IObserver observer)
    {
        if (observer is not null)
            _observers.Remove(observer);
    }

    public void Notify(string message)
    {
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
}
```

**__Observer.Lib/ISubject.cs__** - SampleObserver class implementing IObserver Interface. This class receives a WebSocket class on its constructor.

```CSharp
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
```

## Background Service

This background service injects ISubject interface, so it notifies the clients throught this interface.

```CSharp
using Observer.Lib;

namespace WebScoker.Api;

internal sealed class NotifierBackGroundService(ISubject subject) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000);

            subject.Notify(DateTime.Now.ToString());
        }
    }
}
```

## WebApi Service

The Program.cs from the startup project will inject ISubject interface through **Instance** property.

```CSharp
builder.Services.AddSingleton<ISubject>(SampleSubject.Instance);
````

**__WebSocket.Api/Program.cs__** - Startup project

```CSharp
using Observer.Lib;
using WebScoker.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISubject>(SampleSubject.Instance);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

builder.Services.AddHostedService<NotifierBackGroundService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets(webSocketOptions);

app.Run();
```

```CSharp
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
```