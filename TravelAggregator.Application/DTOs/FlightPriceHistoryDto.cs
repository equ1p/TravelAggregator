using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Application.DTOs
{
  public class FlightPriceHistoryDto
  {
    public string Origin { get; set; }
    public string Destination { get; set; }
    public List<PricePointDto> PricePoints { get; set; } = new();
  }

  public class PricePointDto
  {
    public string Airline { get; set; }
    public decimal PriceUSD { get; set; }
    public decimal OriginalPrice { get; set; }
    public string OriginalCurrency { get; set; }
    public DateTime CapturedAt { get; set; }

  }
}
