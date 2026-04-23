using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.Adapters
{
  public class AmadeusAdapter : ITravelProvider
  {
    private readonly HttpClient _httpClient;
    public AmadeusAdapter(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<IEnumerable<FlightDto>> SearchFlightAsync(string origin, string destination, string date)
    {
      var response = await _httpClient.GetAsync($"$\"/v2/shopping/flight-offers?originLocationCode={{origin}}&destinationLocationCode={{destination}}&departureDate={{date}}&adults=1\"");
      response.EnsureSuccessStatusCode();

      var jsonResponse = await response.Content.ReadAsStringAsync();
      return ParseAmadeusResponse(jsonResponse);
    }

    private IEnumerable<FlightDto> ParseAmadeusResponse(string json)
    {
      return new List<FlightDto>();
    }
  }
}
