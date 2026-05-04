using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Infrastructure.Services
{
  public class FlightPriceService : IFlightPriceService
  {
    private readonly ITravelProvider _travelProvider;
    private readonly IFlightPriceRepository _priceRepository;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<FlightPriceService> _logger;

    public FlightPriceService(
      ITravelProvider travelProvider,
      IFlightPriceRepository priceRepository,
      ICurrencyService currencyService,
      ILogger<FlightPriceService> logger)
    {
      _travelProvider = travelProvider;
      _priceRepository = priceRepository;
      _currencyService = currencyService;
      _logger = logger;
    }

    public async Task<IEnumerable<FlightDto>> SearchAndRecordAsync(string origin, string destination, string date)
    {
      var flights = (await _travelProvider.SearchFlightAsync(origin, destination, date)).ToList();

      var records = new List<FlightPriceRecord>();
      foreach (var flight in flights) 
      {
        decimal normalizedUSD;
        try
        {
          normalizedUSD = await _currencyService.ConvertAsync(
            flight.Price, flight.Currency, "USD");
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Currency conversion failed for {Currency}, using original price as USD fallback.",
            flight.Currency);
          normalizedUSD = flight.Price;
        }

        records.Add(new FlightPriceRecord
        {
          Origin = origin,
          Destination = destination,
          Airline = flight.Airline,
          OriginalPrice = flight.Price,
          OriginalCurrency = flight.Currency,
          NormalizedPriceUSD = normalizedUSD,
          DepartureDate = flight.DepartureTime,
          CapturedAt = DateTime.UtcNow
        });
      }
      if (records.Count > 0)
      {
        await _priceRepository.SavePriceSnapshotsAsync(records);
        _logger.LogInformation("Persisted {Count} price snapshots for {Origin}->{Destination}.",
          records.Count, origin, destination);
      }

      return flights;
    }

    public async Task<IEnumerable<FlightPriceRecord>> GetPriceHistoryAsync(string origin, string destination, DateTime from, DateTime to)
    {
      return await _priceRepository.GetPriceSnapshotsAsync(origin, destination, from, to);
    }

  }
}
