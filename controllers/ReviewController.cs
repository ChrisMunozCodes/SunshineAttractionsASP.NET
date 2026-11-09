using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Models;

namespace SunshineAttractions.Controllers;

[Authorize, Route("review")]
public sealed class ReviewController(MongoContext database) : Controller
{
    [HttpPost("createReview")]
    public async Task<IActionResult> CreateReview(ReviewInput input, List<IFormFile>? files, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId) || string.IsNullOrWhiteSpace(input.PageName) || string.IsNullOrWhiteSpace(input.Description) || input.Rating is < 1 or > 5)
        {
            TempData["Error"] = "A rating and review description are required.";
            return Redirect($"/page/{Uri.EscapeDataString(input.PageName)}");
        }
        if (files?.Any(file => file.Length > 0) == true)
        {
            TempData["Error"] = "Image uploads are not currently supported. Please submit your review without images.";
            return Redirect($"/page/{Uri.EscapeDataString(input.PageName)}");
        }
        await database.Reviews.InsertOneAsync(new Review
        {
            Page = input.PageName,
            UserId = userId,
            ReviewedBy = [userId],
            Rating = input.Rating,
            Description = input.Description.Trim(),
            Likes = 0
        }, cancellationToken: cancellationToken);
        return Redirect($"/page/{Uri.EscapeDataString(input.PageName)}");
    }

    [HttpPost("deleteReview/{id}")]
    public async Task<IActionResult> DeleteReview(string id, CancellationToken cancellationToken)
    {
        if (this.TryGetUserId(out var userId) && ObjectId.TryParse(id, out var reviewId))
        {
            var review = await database.Reviews.Find(x => x.Id == reviewId).FirstOrDefaultAsync(cancellationToken);
            if (review?.UserId == userId)
            {
                await database.Comments.DeleteManyAsync(x => x.ReviewId == reviewId, cancellationToken);
                await database.Reviews.DeleteOneAsync(x => x.Id == reviewId, cancellationToken);
            }
        }
        return Redirect("/your-reviews");
    }

    [HttpPost("likePost/{id}")]
    public async Task<IActionResult> LikePost(string id, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId) || !ObjectId.TryParse(id, out var reviewId)) return Unauthorized();
        var review = await database.Reviews.Find(x => x.Id == reviewId).FirstOrDefaultAsync(cancellationToken);
        if (review is null) return NotFound();
        var hadLiked = review.LikedBy.Contains(userId);
        var update = hadLiked
            ? Builders<Review>.Update.Inc(x => x.Likes, -1).Pull(x => x.LikedBy, userId)
            : Builders<Review>.Update.Inc(x => x.Likes, 1).AddToSet(x => x.LikedBy, userId);
        await database.Reviews.UpdateOneAsync(x => x.Id == reviewId, update, cancellationToken: cancellationToken);
        return Json(new { liked = hadLiked, likes = Math.Max(0, review.Likes + (hadLiked ? -1 : 1)) });
    }

}

public sealed class ReviewInput
{
    [FromForm(Name = "pageName")] public string PageName { get; set; } = "";
    [FromForm(Name = "rating")] public int Rating { get; set; }
    [FromForm(Name = "desc")] public string Description { get; set; } = "";
}
