using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace CodeOps.ConfigurationAsCode.Providers.Azure;

internal class AppConfigurationRefresher : BackgroundService
{
    private readonly IConfigurationRefresherProvider _refresherProvider;
    private readonly ConfigurationRefresherDelay.Get _refresherDelay;

    public AppConfigurationRefresher(
        IConfigurationRefresherProvider refresherProvider,
        ConfigurationRefresherDelay.Get refresherDelay)
    {
        _refresherProvider = refresherProvider;
        _refresherDelay = refresherDelay;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                foreach (var refresher in _refresherProvider.Refreshers)
                {
                    await refresher.TryRefreshAsync(stoppingToken);
                }

                await _refresherDelay()
                    .Match(
                        v => Task.Delay(v.ValidValue, stoppingToken),
                        _ => Task.Delay(1000, stoppingToken));

            }
            catch (Exception e)
            {
                //Ignore
                Console.WriteLine(e);
            }
        }
    }
}