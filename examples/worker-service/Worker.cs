using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Safaricom.Daraja;

namespace WorkerServiceExample;

/// <summary>
/// Periodically exercises the Daraja OAuth token cache - a stand-in for any recurring job
/// (e.g. polling the Pull Transactions API for a shortcode Safaricom has approved for it,
/// or periodically reconciling standing orders) that needs a long-lived, thread-safe client.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DarajaClient _client;

    public Worker(ILogger<Worker> logger, DarajaClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daraja background worker running at: {Time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // GetValidAccessTokenAsync caches the token and only calls Daraja again once it
                // is close to expiring, so this is cheap to call on every tick.
                await _client.Auth.GetValidAccessTokenAsync(stoppingToken);
                _logger.LogInformation("Daraja credentials are valid as of {Time}.", DateTimeOffset.Now);

                // TODO: swap in your own recurring job here, e.g.:
                // var transactions = await _client.PullApi.QueryAsync(new PullApiQueryRequest { ... }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Worker failed to validate Daraja credentials.");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
