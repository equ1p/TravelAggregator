using Microsoft.Extensions.Caching.Memory;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Infrastructure.Services;
using Xunit;

namespace TravelAggregator.Tests.Infrastructure.Services;

public class CurrencyServiceTests
{
  private static (CurrencyService service, IMemoryCache cache) CreateService(
    Dictionary<string, decimal>? rates = null)
  {
    var cache = new MemoryCache(new MemoryCacheOptions());

    if (rates != null)
    {
      var dto = new ExchangeRatesDto
      {
        BaseCurrency = "USD",
        LastUpdated = DateTime.UtcNow,
        Rates = rates
      };
      cache.Set("LatestExchangeRates", dto);
    }

    var service = new CurrencyService(cache);
    return (service, cache);
  }

  [Fact]
  public async Task ConvertAsync_SameCurrency_ReturnsOriginalAmount()
  {
    var (service, _) = CreateService();
    var result = await service.ConvertAsync(100m, "USD", "USD");
    Assert.Equal(100, result);
  }

  [Fact]
  public async Task ConvertAsync_ShouldConvertCorrectly_WhenRatesCached()
  {
    var (service, _) = CreateService(new Dictionary<string, decimal>
    {
      { "USD", 1.0m },
      { "EUR", 0.85m },
      { "GBP", 0.73m }
    });

    var result = await service.ConvertAsync(100m, "EUR", "GBP");

    Assert.Equal(85.88m, result);
  }

  [Fact]
  public async Task ConvertAsync_ShouldThrow_WhenRatesNotCached()
  {
    var (service, _) = CreateService();

    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConvertAsync(100m, "EUR", "GBP"));

    Assert.Contains("unavailable", ex.Message);
  }

  [Fact]
  public async Task ConvertAsync_ShouldThrow_WhenFromCurrencyUnsupported()
  {
    var (service, _) = CreateService(new Dictionary<string, decimal>
    {
      { "USD", 1.0m },
      { "GPB", 0.73m }
    });

    var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ConvertAsync(100m, "XYZ", "GBP"));

    Assert.Contains("XYZ", ex.Message);
  }

  [Fact]
  public async Task ConvertAsync_ShouldThrow_WhenToCurrencyUnsupported()
  {
    var (service, _) = CreateService(new Dictionary<string, decimal>
    {
      { "USD", 1.0m },
      { "EUR", 0.85m }
    });

    var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ConvertAsync(100m, "EUR", "ABC"));

    Assert.Contains("ABC", ex.Message);
  }

  [Fact]
  public async Task ConvertAsync_ShouldRoundToTwoDecimals()
  {
    var (service, _) = CreateService(new Dictionary<string, decimal>
    {
      { "USD", 1.0m },
      { "EUR", 0.85m },
      {"JPY", 149.50m}
    });

    var result = await service.ConvertAsync(100m, "EUR", "JPY");

    var decimalPlaces = BitConverter.GetBytes(
      decimal.GetBits(result)[3])[2];

    Assert.True(decimalPlaces <= 2,
      $"Expected <= 2 decimal places, git {decimalPlaces}, (value: {result})");
  }
}
