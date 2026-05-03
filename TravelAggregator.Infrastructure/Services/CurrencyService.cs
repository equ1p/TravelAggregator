using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.Services
{
  public class CurrencyService : ICurrencyService
  {
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "LatestExchangeRates";

    public CurrencyService(IMemoryCache memoryCache)
    {
      _memoryCache = memoryCache;
    }

    public Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
      if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
      {
        return Task.FromResult(amount);
      }

      if (!_memoryCache.TryGetValue(CacheKey, out ExchangeRatesDto cachedRates) || cachedRates == null)
      {
        throw new InvalidOperationException("Exchange rates are currently unavailable.");
      }

      var fromUpper = fromCurrency.ToUpperInvariant();
      var toUpper = toCurrency.ToUpperInvariant();

      if ( !cachedRates.Rates.TryGetValue(fromUpper, out var rateFrom))
      {
          throw new ArgumentException($"Currency not supported: {fromUpper}");
      }

      if (!cachedRates.Rates.TryGetValue(toUpper, out var rateTo))
      {
          throw new ArgumentException($"Currency not supported: {toUpper}");
      }

      var amountInUsd = amount / rateFrom;
      var convertedAmount = amountInUsd * rateTo;

      return Task.FromResult(Math.Round(convertedAmount, 2));
    }
  }
}
