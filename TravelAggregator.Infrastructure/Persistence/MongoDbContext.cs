using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Infrastructure.Persistence
{
  public class MongoDbContext
  {
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
      var client = new MongoClient(connectionString);
      _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<FlightPriceRecord> FlightPrices
      => _database.GetCollection<FlightPriceRecord>("FlightPrices");

    public IMongoCollection<BsonDocument> FlightResponses
      => _database.GetCollection<BsonDocument>("FlightResponses");

    public IMongoCollection<BsonDocument> CurrencyRateSnapshots
      => _database.GetCollection<BsonDocument>("CurrencyRateSnapshots");

    public async Task<List<BsonDocument>> GetFlightsByDestinationAsync(string destination)
    {
      var filter = Builders<BsonDocument>.Filter.Eq("destination", destination);
      return await FlightResponses.Find(filter).ToListAsync();
    }

    public async Task SaveRawResponseAsync(string jsonResponse)
    {
      var document = BsonDocument.Parse(jsonResponse);
      await FlightResponses.InsertOneAsync(document);
    }
  }
}
