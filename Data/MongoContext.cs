using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SunshineAttractions.Models;

namespace SunshineAttractions.Data;

public sealed class MongoContext
{
    public MongoContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:MongoDb must be configured.");

        var databaseName = MongoUrl.Create(connectionString).DatabaseName;
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("The MongoDB connection string must include a database name.");

        var database = new MongoClient(connectionString).GetDatabase(databaseName);
        Users = database.GetCollection<User>("users");
        Reviews = database.GetCollection<Review>("reviews");
        Comments = database.GetCollection<Comment>("comments");
        Markers = database.GetCollection<Marker>("markers");
        ParkCircles = database.GetCollection<ParkCircle>("parkCircles");
    }

    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Review> Reviews { get; }
    public IMongoCollection<Comment> Comments { get; }
    public IMongoCollection<Marker> Markers { get; }
    public IMongoCollection<ParkCircle> ParkCircles { get; }
}
