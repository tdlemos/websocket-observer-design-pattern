
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
