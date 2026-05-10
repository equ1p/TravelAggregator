using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using RichardSzalay.MockHttp;
using TravelAggregator.Infrastructure.Adapters;
using Xunit;


namespace TravelAggregator.Tests.Infrastructure.Adapters;

public class DuffelAdapterTests
{
  private static Mock<IConfiguration> CreateConfigMock()
  {
    var mock = new Mock<IConfiguration>();
    mock.Setup(x => x["DuffelApi:BaseUrl"]).Returns("https://api.duffel.com");
    mock.Setup(x => x["DuffelApi:AccessToken"]).Returns("test_token");
    return mock;
  }

  [Fact]
  public async Task SearchFlightAsync_ShouldReturnParsedFlights_WhenApiReturnsSuccess()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    var fakeResponse = new
    {
      data = new
      {
        offers = new[]
        {
          new
          {
            total_amount = "150.50",
            total_currency = "GBP",
            owner = new { iata_code = "BA" },
            slices = new[]
            {
              new { segments = new[] { new { departing_at = "2026-12-01T10:00:00" } } }
            }
          }
        }
      }
    };

    mockHttp
      .When("https://api.duffel.com/air/offer_requests?return_offers=true")
      .Respond("application/json", JsonSerializer.Serialize(fakeResponse));

    var HttpClient = new HttpClient(mockHttp);
    var adapter = new DuffelAdapter(HttpClient, configMock.Object);

    var result = await adapter.SearchFlightAsync("LHR", "JSK", "2026-12-01");

    Assert.NotNull(result);
    var flight = Assert.Single(result);
    Assert.Equal("BA", flight.Airline);
    Assert.Equal(150.50m, flight.Price);
    Assert.Equal("GBP", flight.Currency);
    Assert.Equal("2026-12-01T10:00:00", flight.DepartureTime);
    Assert.Equal("LHR", flight.Origin);
    Assert.Equal("JSK", flight.Destination);
  }

  [Fact]
  public async Task SearchFlightAsync_ShouldReturnEmpty_WhenNoOffers()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    var fakeResponse = new { data = new { offers = Array.Empty<object>() } };

    mockHttp
      .When("https://api.duffel.com/air/offer_requests?return_offers=true")
      .Respond("application/json", JsonSerializer.Serialize(fakeResponse));

    var HttpClient = new HttpClient(mockHttp);
    var adapter = new DuffelAdapter(HttpClient, configMock.Object);

    var result = await adapter.SearchFlightAsync("LHR", "JSK", "2026-12-01");

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task SearchFlightAsync_ShouldThrow_WhenApiReturnsError()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    mockHttp
      .When("https://api.duffel.com/air/offer_requests?return_offers=true")
      .Respond(HttpStatusCode.InternalServerError);

    var HttpClient = new HttpClient(mockHttp);
    var adapter = new DuffelAdapter(HttpClient, configMock.Object);

    await Assert.ThrowsAsync<HttpRequestException>(
      () => adapter.SearchFlightAsync("LHR", "JSK", "2026-12-01"));
  }

  [Fact]
  public async Task SearchFlightAsync_ShouldParseMultipleOffers()
  {
    var configMock = CreateConfigMock();
    var mockHttp = new MockHttpMessageHandler();

    var fakeResponse = new
    {
      data = new
      {
        offers = new[]
        {
          new
          {
            total_amount = "150.50",
            total_currency = "USD",
            owner = new { iata_code = "AA" },
            slices = new[] { new { segments = new[] { new { departing_at = "2026-12-01T08:00:00" } } } }
          },
          new
          {
            total_amount = "200.00",
            total_currency = "EUR",
            owner = new { iata_code = "LH" },
            slices = new[] { new { segments = new[] { new { departing_at = "2026-12-01T12:00:00" } } } }
          },
          new
          {
            total_amount = "300.00",
            total_currency = "GBP",
            owner = new { iata_code = "BA" },
            slices = new[] { new { segments = new[] { new { departing_at = "2026-12-01T16:00:00" } } } }
          }
        }
      }
    };

    mockHttp
      .When("https://api.duffel.com/air/offer_requests?return_offers=true")
      .Respond("application/json", JsonSerializer.Serialize(fakeResponse));

    var HttpClient = new HttpClient(mockHttp);
    var adapter = new DuffelAdapter(HttpClient, configMock.Object);

    var result = (await adapter.SearchFlightAsync("WAW", "LHR", "2026-12-01")).ToList();

    Assert.Equal(3, result.Count);
    Assert.Equal("AA", result[0].Airline);
    Assert.Equal("LH", result[1].Airline);
    Assert.Equal("BA", result[2].Airline);

    Assert.All(result, f => Assert.Equal("WAW", f.Origin));
    Assert.All(result, f => Assert.Equal("LHR", f.Destination));
  }

}


