using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.Adapters
{
    public class DuffelAdapter : ITravelProvider
    {
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public DuffelAdapter(HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            var baseUrl = _configuration["DuffelApi:BaseUrl"] ?? "https://api.duffel.com";
            _httpClient.BaseAddress = new Uri(baseUrl);
            
            var token = _configuration["DuffelApi:AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            // Duffel requires a version header
            _httpClient.DefaultRequestHeaders.Add("Duffel-Version", "beta");
        }

        public async Task<IEnumerable<FlightDto>> SearchFlightAsync(string origin, string destination, string date)
        {
            // Prepare the request payload according to Duffel's Offer Requests schema
            var requestPayload = new
            {
                data = new
                {
                    slices = new[]
                    {
                        new
                        {
                            origin = origin,
                            destination = destination,
                            departure_date = date
                        }
                    },
                    passengers = new[]
                    {
                        new { type = "adult" }
                    },
                    cabin_class = "economy"
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/air/offer_requests?return_offers=true", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return ParseDuffelResponse(jsonResponse);
        }

        private IEnumerable<FlightDto> ParseDuffelResponse(string json)
        {
            var flights = new List<FlightDto>();
            using var doc = JsonDocument.Parse(json);
            
            // The response contains "data", which has an "offers" array
            if (doc.RootElement.TryGetProperty("data", out var data) && 
                data.TryGetProperty("offers", out var offers))
            {
                foreach (var offer in offers.EnumerateArray())
                {
                    var priceStr = offer.GetProperty("total_amount").GetString();
                    var price = decimal.Parse(priceStr ?? "0", System.Globalization.CultureInfo.InvariantCulture);

                    var owner = offer.GetProperty("owner");
                    var airline = owner.GetProperty("iata_code").GetString(); // Or "name" if preferred

                    var slices = offer.GetProperty("slices");
                    var firstSlice = slices[0];
                    var segments = firstSlice.GetProperty("segments");
                    var firstSegment = segments[0];

                    var departure = firstSegment.GetProperty("departing_at").GetString();

                    flights.Add(new FlightDto
                    {
                        Airline = airline ?? "Unknown",
                        Price = price,
                        DepartureTime = departure ?? "Unknown"
                    });
                }
            }

            return flights;
        }
    }
}
