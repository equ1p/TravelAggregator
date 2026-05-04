using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Application.Interfaces
{
  public interface IFlightPriceRepository
  {
     Task SavePriceSnapshotAsync(FlightPriceRecord record);
    Task SavePriceSnapshotsAsync(IEnumerable<FlightPriceRecord> records);
    Task<IEnumerable<FlightPriceRecord>> GetPriceSnapshotsAsync(
      string origin, string destination, DateTime from, DateTime to);
  }
}

