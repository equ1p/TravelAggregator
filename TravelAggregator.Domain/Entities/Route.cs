using System;

namespace TravelAggregator.Domain.Entities
{
  public class Route : BaseEntity
  {
    public string OriginCity { get; private set; }
    public string DestinationCity { get; private set; }
    public DateTime DepartureDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }

    public Route(string originCity, string destinationCity, DateTime departureDate, DateTime? returnDate = null)
    {
      OriginCity = originCity;
      DestinationCity = destinationCity;
      DepartureDate = departureDate;
      ReturnDate = returnDate;
    }
  }
}
