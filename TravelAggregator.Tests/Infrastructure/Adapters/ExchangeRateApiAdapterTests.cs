using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;
using TravelAggregator.Infrastructure.Adapters;
using Xunit;

namespace TravelAggregator.Tests.Infrastructure.Adapters;

public class ExchangeRateApiAdapterTests
{
  private const string TestApiKey = "test_api_key_123";

  private static Mock<IConfiguration> CreateConfigMock(string? apiKey = TestApiKey)
  {
    var mock = new Mock<IConfiguration>();
    mock.Setup(x => x["ExchangeRateApiKey"]).Returns(apiKey!);
    return mock;
  }

  [Fact]
  public async Task GetLatestRatesAsync_ShouldReturnRates_WhenApiSucceeds()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    var fakeResponse = new
    {
      result = "sucess",
      conversion_rates = new Dictionary<string, decimal>
      {
        { "USD", 1.0m },
        { "EUR", 0.85m },
        { "GBP", 0.73m },
        { "PLN", 4.05m }
      }
    };

    mockHttp
      .When($"https://v6.exchangerate-api.com/v6/{TestApiKey}/latest/USD")
      .Respond("application/json", JsonSerializer.Serialize(fakeResponse));

    var httpClient = new HttpClient(mockHttp);
    var adapter = new ExchangeRateApiAdapter(httpClient, configMock.Object);

    var result = await adapter.GetLatestRatesAsync();

    Assert.NotNull(result);
    Assert.Equal("USD", result.BaseCurrency);
    Assert.Equal(4, result.Rates.Count);
    Assert.Equal(0.85m, result.Rates["EUR"]);
    Assert.Equal(0.73m, result.Rates["GBP"]);
    Assert.Equal(4.05m, result.Rates["PLN"]);
  }

  [Fact]
  public async Task GetLatestRatesAsync_ShouldThrow_WhenApiReturnsError()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    mockHttp
      .When($"https://v6.exchangerate-api.com/v6/{TestApiKey}/latest/USD")
      .Respond(HttpStatusCode.Forbidden);

    var httpClient = new HttpClient(mockHttp);
    var adapter = new ExchangeRateApiAdapter(httpClient, configMock.Object);

    await Assert.ThrowsAsync<HttpRequestException>(() => adapter.GetLatestRatesAsync());
  }

  [Fact]
  public void Constructor_ShouldThrow_WhenApiKeyMissing()
  {
    var configMock = new Mock<IConfiguration>();
    configMock.Setup(x => x["ExchangeRateApiKey"]).Returns((string?)null);

    var httpClient = new HttpClient();

    Assert.Throws<ArgumentNullException>(() => new ExchangeRateApiAdapter(httpClient, configMock.Object));

  }
}
