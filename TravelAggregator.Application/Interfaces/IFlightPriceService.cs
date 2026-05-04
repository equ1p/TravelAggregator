using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Application.Interfaces
{
  public interface IFlightPriceService
  {
    Task<IEnumerable<FlightDto>> SearchAndRecordAsync
      (string origin, string destination, string date);

    Task<IEnumerable<FlightPriceRecord>> GetPriceHistoryAsync
      (string origin, string destination, DateTime from, DateTime to);
  }
}
