using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Application.DTOs
{
  public class ExchangeRatesDto
  {
    public string BaseCurrency { get; set; } = "USD";
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new();
  }
}
