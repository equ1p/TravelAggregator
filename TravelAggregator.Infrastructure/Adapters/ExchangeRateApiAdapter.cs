using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.Adapters
{
  public class ExchangeRateApiAdapter : ICurrencyRateProvider
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ExchangeRateApiAdapter(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["ExchangeRateApiKey"] ?? throw new ArgumentNullException("ExchangeRateApiKey configuration is missing.");
    }

    public async Task<ExchangeRatesDto> GetLatestRatesAsync()
    {
      var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/USD";
      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      using var document = JsonDocument.Parse(content);
      var root = document.RootElement;

      var result = new ExchangeRatesDto
      {
        BaseCurrency = "USD",
        LastUpdated = DateTime.UtcNow,
      };

      var ratesElement = root.GetProperty("conversion_rates");
      foreach (var property in ratesElement.EnumerateObject())
      {
        result.Rates[property.Name] = property.Value.GetDecimal();
      }

      return result;
    }
  }
}
