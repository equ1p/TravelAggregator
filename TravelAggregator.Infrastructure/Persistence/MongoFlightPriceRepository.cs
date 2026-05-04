using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Infrastructure.Persistence
{
  public class MongoFlightPriceRepository : IFlightPriceRepository
  {
    private readonly MongoDbContext _context;

    public MongoFlightPriceRepository(MongoDbContext context)
    {
      _context = context;
    }

    public async Task SavePriceSnapshotAsync(FlightPriceRecord record)
    {
      await _context.FlightPrices.InsertOneAsync(record);
    }

    public async Task SavePriceSnapshotsAsync(IEnumerable<FlightPriceRecord> records)
    {
      var list = records.ToList();
      if (list.Count == 0) return;
      
      await _context.FlightPrices.InsertManyAsync(list);
    }
    public async Task<IEnumerable<FlightPriceRecord>> GetPriceSnapshotsAsync(string origin, string destination, DateTime from, DateTime to)
    {
      var filter = Builders<FlightPriceRecord>.Filter.And(
        Builders<FlightPriceRecord>.Filter.Eq(r => r.Origin, origin),
        Builders<FlightPriceRecord>.Filter.Eq(r => r.Destination, destination),
        Builders<FlightPriceRecord>.Filter.Gte(r => r.CapturedAt, from),
        Builders<FlightPriceRecord>.Filter.Lte(r => r.CapturedAt, to)
        );

      var sort = Builders<FlightPriceRecord>.Sort.Ascending(r => r.CapturedAt);

      return await _context.FlightPrices
        .Find(filter)
        .Sort(sort)
        .ToListAsync();
    }
  }
}
