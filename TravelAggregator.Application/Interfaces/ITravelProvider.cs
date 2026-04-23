using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using TravelAggregator.Application.DTOs;

namespace TravelAggregator.Application.Interfaces
{
  public interface ITravelProvider
  {
    Task<IEnumerable<FlightDto>> SearchFlightAsync(string origin, string destination, string date);

  }
}
