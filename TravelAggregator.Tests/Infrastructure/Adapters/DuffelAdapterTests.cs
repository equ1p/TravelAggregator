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
    [Fact]
    public async Task SearchFlightAsync_ShouldReturnParsedFlights_WhenApiReturnsSuccess()
    {
      // 1. Arrange: Мокаємо Configuration
      var configMock = new Mock<IConfiguration>();
      configMock.Setup(x => x["DuffelApi:BaseUrl"]).Returns("https://api.duffel.com");
      configMock.Setup(x => x["DuffelApi:AccessToken"]).Returns("test_token");
      // 2. Arrange: Мокаємо HttpClient
      var mockHttp = new MockHttpMessageHandler();

      // Фейкова відповідь від Duffel API
      var fakeResponse = new
      {
        data = new {
          offers = new[] {
            new {
              total_amount = "150.50",
              owner = new { iata_code = "BA" },
              slices = new[] {
                new { segments = new[] { new { departing_at = "2024-12-01T10:00:00" } } }
              }
            }
          }
        }
      };
      mockHttp.When("https://api.duffel.com/air/offer_requests?return_offers=true")
        .Respond("application/json", JsonSerializer.Serialize(fakeResponse));
      var httpClient = new HttpClient(mockHttp);
      var adapter = new DuffelAdapter(httpClient, configMock.Object);
      // 3. Act: Викликаємо метод
      var result = await adapter.SearchFlightAsync("LHR", "JFK", "2024-12-01");
      // 4. Assert: Перевіряємо результат
      Assert.NotNull(result);
      var flight = Assert.Single(result);
      Assert.Equal("BA", flight.Airline);
      Assert.Equal(150.50m, flight.Price);
      Assert.Equal("2024-12-01T10:00:00", flight.DepartureTime);
    }
}


