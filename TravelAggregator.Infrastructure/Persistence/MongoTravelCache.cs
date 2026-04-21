using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Infrastructure.Persistence
{
  public class MongoTravelCache
  {
    private readonly IMongoCollection<BsonDocument> _collection;

    public MongoTravelCache(string connectionString, string databaseName)
    {
      var client = new MongoClient(connectionString);
      var database = client.GetDatabase(databaseName);
      _collection = database.GetCollection<BsonDocument>("FlightResponses");
    }

    public async Task<List<BsonDocument>> GetFlightsByDestinationAsync(string destination)
    {
      var filter = Builders<BsonDocument>.Filter.Eq("destination", destination);
      return await _collection.Find(filter).ToListAsync();
    }

    public async Task SaveRawResponcesAsync(string jsonResponse)
    {
      var document = BsonDocument.Parse(jsonResponse);
      await _collection.InsertOneAsync(document);
    }
  }
}
