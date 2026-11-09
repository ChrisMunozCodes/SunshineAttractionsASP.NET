using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SunshineAttractions.Models;

public sealed class User
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonElement("profilePic")] public string ProfilePic { get; set; } = "/assets/image/Sunshine%20Attractions-logo/default-profile-pic.png";
    [BsonElement("userName")] public string UserName { get; set; } = "";
    [BsonElement("email")] public string Email { get; set; } = "";
    [BsonElement("password")] public string PasswordHash { get; set; } = "";
}

public sealed class Review
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonElement("user")] public ObjectId UserId { get; set; }
    [BsonElement("page")] public string Page { get; set; } = "";
    [BsonElement("reviewedBy")] public List<ObjectId> ReviewedBy { get; set; } = [];
    [BsonElement("rating")] public int Rating { get; set; }
    [BsonElement("desc")] public string Description { get; set; } = "";
    [BsonElement("images")] public List<string> Images { get; set; } = [];
    [BsonElement("cloudinaryIds")] public List<string> CloudinaryIds { get; set; } = [];
    [BsonElement("likes")] public int Likes { get; set; }
    [BsonElement("likedBy")] public List<ObjectId> LikedBy { get; set; } = [];
    [BsonElement("comments")] public List<ObjectId> Comments { get; set; } = [];
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Comment
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonElement("user")] public ObjectId UserId { get; set; }
    [BsonElement("page")] public string Page { get; set; } = "";
    [BsonElement("commentDesc")] public string Description { get; set; } = "";
    [BsonElement("review")] public ObjectId ReviewId { get; set; }
    [BsonElement("likes")] public int Likes { get; set; }
    [BsonElement("likedBy")] public List<ObjectId> LikedBy { get; set; } = [];
    [BsonElement("dislikes")] public int Dislikes { get; set; }
    [BsonElement("dislikedBy")] public List<ObjectId> DislikedBy { get; set; } = [];
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class Marker
{
    [BsonId] public ObjectId Id { get; set; }
    [BsonElement("name")] public string Name { get; set; } = "";
    [BsonElement("lat")] public double Latitude { get; set; }
    [BsonElement("long")] public List<double> Longitude { get; set; } = [];
    [BsonElement("park")] public string Park { get; set; } = "";
    [BsonElement("wait")] public string? Wait { get; set; }
}
