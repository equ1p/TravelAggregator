using Microsoft.AspNetCore.Mvc;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.WebAPI.Controllers
{
  [ApiController]
  [Route("api/flights")]
  public class FlightPriceController : ControllerBase
  {
    private readonly IFlightPriceService _flightPriceService;

    public FlightPriceController(IFlightPriceService flightPriceService)
    {
      _flightPriceService = flightPriceService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFlights(
      [FromQuery] string origin,
      [FromQuery] string destination,
      [FromQuery] string date)
    {
      if (string.IsNullOrWhiteSpace(origin) ||
        string.IsNullOrWhiteSpace(destination) ||
        string.IsNullOrWhiteSpace(date))
      {
        return BadRequest("origin, destination and date are required");
      }

      var flights = await _flightPriceService.SearchAndRecordAsync(origin, destination, date);
      return Ok(flights);
    }

    [HttpGet("price-history")]
    public async Task<IActionResult> GetPriceHostory(
      [FromQuery] string origin,
      [FromQuery] string destination,
      [FromQuery] DateTime from,
      [FromQuery] DateTime to)
    {
      if (string.IsNullOrWhiteSpace(origin) ||
        string.IsNullOrWhiteSpace(destination))
      {
        return BadRequest("origin and destination are required.");
      }

      var records = await _flightPriceService.GetPriceHistoryAsync(origin, destination, from, to);

      var dto = new FlightPriceHistoryDto
      {
        Origin = origin,
        Destination = destination,
        PricePoints = records.Select(r => new PricePointDto
        {
          Airline = r.Airline,
          PriceUSD = r.NormalizedPriceUSD,
          OriginalPrice = r.OriginalPrice,
          OriginalCurrency = r.OriginalCurrency,
          CapturedAt = r.CapturedAt
        }).ToList()
      };

      return Ok(dto);
    }
  }
}
