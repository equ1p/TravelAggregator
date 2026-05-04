using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.BackgroundJobs
{
  public class FlightPriceSyncBackgroundService : BackgroundService
  {
    private readonly ILogger<FlightPriceSyncBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private static readonly TimeSpan PollingInterval = TimeSpan.FromHours(1);
    private static readonly List<(string Origin, string Destination, string Date)> TrackedRoutes = new()
    {
      ("WAW", "LHR", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")),
      ("WAW", "CDG", DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd")),
    };

    public FlightPriceSyncBackgroundService(
      ILogger<FlightPriceSyncBackgroundService> logger,
      IServiceProvider serviceProvider)
    {
      _logger = logger;
      _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Flight Price Sync Background Service is starting.");

      await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        _logger.LogInformation("Starting periodic flight price capture...");

        foreach (var (origin, destination, date) in TrackedRoutes)
        {
          try
          {
            using var scope = _serviceProvider.CreateScope();
            var flightPriceService = scope.ServiceProvider
              .GetRequiredService<IFlightPriceService>();

            await flightPriceService.SearchAndRecordAsync(origin, destination, date);

            _logger.LogInformation(
              "Captured prices for {Origin} -> {Destination} on {Date}.",
              origin, destination, date);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex,
              "Failed to capture prices for {Origin} -> {Destination}.",
              origin, destination);
          }
        }

        await Task.Delay(PollingInterval, stoppingToken);
      }
    }
  }
}
