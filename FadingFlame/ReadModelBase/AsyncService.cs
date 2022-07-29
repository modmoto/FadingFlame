using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FadingFlame.ReadModelBase;

public class AsyncService<THandler> : IHostedService where THandler : IAsyncUpdatable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Task _executingTask;
    private readonly CancellationTokenSource _stoppingCts = new();

    public AsyncService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_stoppingCts.Token);

        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }

        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var service = scope.ServiceProvider.GetService<THandler>();
                    var versionRepository = scope.ServiceProvider.GetService<IVersionRepository>();
                    if (service != null && versionRepository != null)
                    {
                        var version = await versionRepository.GetLastVersion<THandler>();
                        var newVersion = await service.Update(version);
                        await versionRepository.SaveLastVersion<THandler>(newVersion);
                    }
                }
                    
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<THandler>>();
                    logger.LogError(e, "Some Readmodelhandler is dying");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
        while (!stoppingToken.IsCancellationRequested);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _stoppingCts.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
}