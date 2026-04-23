using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Application.DTOs
{
  public class FlightDto
  {
    public string Airline { get; set; }
    public decimal Price { get; set; }
    public string DepartureTime { get; set; }
  }
}
