using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.DTOs;
using TravelAggregator.Application.Interfaces;

namespace TravelAggregator.Infrastructure.Persistence
{
  public class MongoCurrencyRepository : ICurrencyRepository
  {
    private readonly MongoDbContext _context;

    public MongoCurrencyRepository(MongoDbContext context)
    {
      _context = context;
    }

    public async Task SaveRatesSnapshotAsync(ExchangeRatesDto ratesDto)
    {
      var document = new BsonDocument
      {
        { "baseCurrency", ratesDto.BaseCurrency},
        { "lastUpdated", ratesDto.LastUpdated },
        { "rates", new BsonDocument(
          ratesDto.Rates.Select(kvp =>
            new BsonElement(kvp.Key, BsonValue.Create((double)kvp.Value))
            )
          )
        }
      };

      await _context.CurrencyRateSnapshots.InsertOneAsync(document);
    }
  }
}
