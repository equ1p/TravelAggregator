using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.BackgroundJobs
{
  public class CurrencySyncBackgroundService : BackgroundService
  {
    private readonly ILogger<CurrencySyncBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "LatestExchangeRates";

    public CurrencySyncBackgroundService(
      ILogger<CurrencySyncBackgroundService> logger,
      IServiceProvider serviceProvider,
      IMemoryCache memoryCache)
    {
      _logger = logger;
      _serviceProvider = serviceProvider;
      _memoryCache = memoryCache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Currency Sync Background service is starting.");

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          _logger.LogInformation("Fetching latest exchange rates from provider...");
          using var scope = _serviceProvider.CreateScope();
          var ratesProvider = scope.ServiceProvider.GetRequiredService<ICurrencyRateProvider>();
          var repository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();

          var latestRates = await ratesProvider.GetLatestRatesAsync();

          _memoryCache.Set(CacheKey, latestRates);
          await repository.SaveRatesSnapshotAsync(latestRates);

          _logger.LogInformation("Successfully updated exchange rates.");
        }
        catch (Exception ex) 
        {
          _logger.LogError(ex, "An error occurred while fetching.");
        }

        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
      }
    }
  }
}
