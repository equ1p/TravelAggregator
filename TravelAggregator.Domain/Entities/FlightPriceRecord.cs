using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TravelAggregator.Domain.Entities
{
  public class FlightPriceRecord
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public string Airline { get; set; }
    public decimal OriginalPrice { get; set; }
    public string OriginalCurrency { get; set; }
    public decimal NormalizedPriceUSD { get; set; }
    public string DepartureDate { get; set; }
    public DateTime CapturedAt { get; set; }
  }
}
