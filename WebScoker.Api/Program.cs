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