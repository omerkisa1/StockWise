using MongoDB.Bson;
using MongoDB.Driver;
using System.Configuration;

namespace StockWise
{
    public static class MongoDBHelper
{
    private static readonly string _connectionString = ConfigurationManager.AppSettings["MongoDBConnection"];
    private static readonly string _databaseName = ConfigurationManager.AppSettings["DatabaseName"];
    private static IMongoDatabase _database;

    // MongoDB Database Bağlantısı
    static MongoDBHelper()
    {
        var client = new MongoClient(_connectionString);
        _database = client.GetDatabase(_databaseName);
    }

    // İstediğiniz koleksiyonu almanız için bir metot
    public static IMongoCollection<BsonDocument> GetCollection(string collectionName)
    {
        return _database.GetCollection<BsonDocument>(collectionName);
    }
}
}