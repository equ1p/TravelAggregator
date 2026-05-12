using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Domain.Entities;
using TravelAggregator.Infrastructure.Services;
using Xunit;

namespace TravelAggregator.Tests.Infrastructure.Services
{
  public class FlightPriceServiceTests
  {
    private readonly Mock<ITravelProvider> _travelProviderMock = new();
    private readonly Mock<IFlightPriceRepository> _priceRepoMock = new();
    private readonly Mock<ICurrencyService> _currencyServiceMock = new();
    private readonly Mock<ILogger<FlightPriceService>> _loggerMock = new();

    private FlightPriceService CreateService() =>
      new FlightPriceService(
        _travelProviderMock.Object,
        _priceRepoMock.Object,
        _currencyServiceMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task SearchAndRecordAsync_ShouldReturnFlights_AndPersistRecords()
    {
      var flights = new List<FlightDto>
      {
        new FlightDto
        {
          Airline = "BA",
          Price = 150.00m,
          Currency = "GBP",
          DepartureTime = "2026-12-01T10:00:00",
          Origin = "LHR",
          Destination = "JFK"
        }
      };

      _travelProviderMock
        .Setup(x => x.SearchFlightAsync("LHR", "JFK", "2026-12-01"))
        .ReturnsAsync(flights);

      _currencyServiceMock
        .Setup(x => x.ConvertAsync(150.00m, "GBP", "USD"))
        .ReturnsAsync(190.50m);

      var service = CreateService();

      var result = (await service.SearchAndRecordAsync("LHR", "JFK", "2026-12-01")).ToList();

      Assert.Single(result);
      Assert.Equal("BA", result[0].Airline);

      _priceRepoMock.Verify(
        x => x.SavePriceSnapshotsAsync(It.Is<IEnumerable<FlightPriceRecord>>(
          records => records.Any(r =>
          r.Origin == "LHR" &&
          r.Destination == "JFK" &&
          r.Airline == "BA" &&
          r.OriginalPrice == 150.00m &&
          r.OriginalCurrency == "GBP" &&
          r.NormalizedPriceUSD == 190.50m))),
        Times.Once);
    }

    [Fact]
    public async Task SearchAndRecordAsync_ShouldUseFallbackPrice_WhenConversionFails()
    {
      var flights = new List<FlightDto>
      {
        new FlightDto
        {
          Airline = "LH",
          Price = 200.00m,
          Currency = "EUR",
          DepartureTime = "2026-12-01T12:00:00",
          Origin = "FRA",
          Destination = "JFK"
        }
      };

      _travelProviderMock
        .Setup(x => x.SearchFlightAsync("FRA", "JFK", "2026-12-01"))
        .ReturnsAsync(flights);

      _currencyServiceMock
        .Setup(x => x.ConvertAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new InvalidOperationException("Exchange rates unavailable"));

      var service = CreateService();
      var result = (await service.SearchAndRecordAsync("FRA", "JFK", "2026-12-01")).ToList();

      Assert.Single(result);

      _priceRepoMock.Verify(
        x => x.SavePriceSnapshotsAsync(It.Is<IEnumerable<FlightPriceRecord>>(
          records => records.Any(r =>
            r.NormalizedPriceUSD == 200.00m &&
            r.OriginalPrice == 200.00m))),
        Times.Once);
    }

    [Fact]
    public async Task SearchAndRecordAsync_ShouldNotPersist_WhenNoFlihtsFound()
    {
      _travelProviderMock
        .Setup(x => x.SearchFlightAsync("XXX", "YYY", "2026-12-01"))
        .ReturnsAsync(new List<FlightDto>());

      var service = CreateService();
      var result = (await service.SearchAndRecordAsync("XXX", "YYY", "2026-12-01")).ToList();

      Assert.Empty(result);
      _priceRepoMock.Verify(x => x.SavePriceSnapshotsAsync(It.IsAny<IEnumerable<FlightPriceRecord>>()), Times.Never);
    }

    [Fact]
    public async Task GetPriceHistoryAsync_ShouldDelegateToRepository()
    {
      var from = new DateTime(2026, 1, 1);
      var to = new DateTime(2026, 12, 31);
      var expectedRecords = new List<FlightPriceRecord>
      {
        new FlightPriceRecord
        { 
          Origin = "LHR",
          Destination = "JFK",
          Airline = "BA",
          OriginalPrice = 150m,
          OriginalCurrency = "GBP",
          NormalizedPriceUSD = 190m,
          DepartureDate = "2026-06-15",
          CapturedAt = new DateTime(2025, 6, 1)
        }
      };

      _priceRepoMock
        .Setup(x => x.GetPriceSnapshotsAsync("LHR", "JFK", from, to))
        .ReturnsAsync(expectedRecords);

      var service = CreateService();
      var result = (await service.GetPriceHistoryAsync("LHR", "JFK", from, to)).ToList();

      Assert.Single(result);
      Assert.Equal("BA", result[0].Airline);
      Assert.Equal(190m, result[0].NormalizedPriceUSD);

      _priceRepoMock.Verify(x => x.GetPriceSnapshotsAsync("LHR", "JFK", from, to), Times.Once);
    }

  }
}
