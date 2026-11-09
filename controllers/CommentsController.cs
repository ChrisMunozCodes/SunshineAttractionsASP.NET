using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Models;

namespace SunshineAttractions.Controllers;

[Authorize, Route("comment")]
public sealed class CommentsController(MongoContext database) : Controller
{
    [HttpPost("createComment/{id}")]
    public async Task<IActionResult> CreateComment(string id, string commentDesc, string pageName, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId) || !ObjectId.TryParse(id, out var reviewId) || string.IsNullOrWhiteSpace(commentDesc)) return BadRequest();
        var comment = new Comment { UserId = userId, ReviewId = reviewId, Page = pageName, Description = commentDesc.Trim(), Likes = 0, Dislikes = 0 };
        await database.Comments.InsertOneAsync(comment, cancellationToken: cancellationToken);
        await database.Reviews.UpdateOneAsync(x => x.Id == reviewId, Builders<Review>.Update.AddToSet(x => x.Comments, comment.Id), cancellationToken: cancellationToken);
        return Redirect($"/page/{Uri.EscapeDataString(pageName)}");
    }

    [HttpPost("deleteComment/{id}")]
    public async Task<IActionResult> DeleteComment(string id, CancellationToken cancellationToken)
    {
        if (this.TryGetUserId(out var userId) && ObjectId.TryParse(id, out var commentId))
        {
            var comment = await database.Comments.Find(x => x.Id == commentId).FirstOrDefaultAsync(cancellationToken);
            if (comment?.UserId == userId)
            {
                await database.Comments.DeleteOneAsync(x => x.Id == commentId, cancellationToken);
                await database.Reviews.UpdateOneAsync(x => x.Id == comment.ReviewId, Builders<Review>.Update.Pull(x => x.Comments, commentId), cancellationToken: cancellationToken);
            }
        }
        return Redirect("/your-comments");
    }

    [HttpPost("likeComment/{id}")]
    public Task<IActionResult> LikeComment(string id, CancellationToken cancellationToken) => Toggle(id, true, cancellationToken);

    [HttpPost("dislikeComment/{id}")]
    public Task<IActionResult> DislikeComment(string id, CancellationToken cancellationToken) => Toggle(id, false, cancellationToken);

    private async Task<IActionResult> Toggle(string id, bool likes, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId) || !ObjectId.TryParse(id, out var commentId)) return Unauthorized();
        var comment = await database.Comments.Find(x => x.Id == commentId).FirstOrDefaultAsync(cancellationToken);
        if (comment is null) return NotFound();
        var previous = likes ? comment.LikedBy.Contains(userId) : comment.DislikedBy.Contains(userId);
        var update = likes
            ? (previous ? Builders<Comment>.Update.Inc(x => x.Likes, -1).Pull(x => x.LikedBy, userId) : Builders<Comment>.Update.Inc(x => x.Likes, 1).AddToSet(x => x.LikedBy, userId))
            : (previous ? Builders<Comment>.Update.Inc(x => x.Dislikes, -1).Pull(x => x.DislikedBy, userId) : Builders<Comment>.Update.Inc(x => x.Dislikes, 1).AddToSet(x => x.DislikedBy, userId));
        await database.Comments.UpdateOneAsync(x => x.Id == commentId, update, cancellationToken: cancellationToken);
        return Json(likes
            ? new { liked = previous, likes = Math.Max(0, comment.Likes + (previous ? -1 : 1)) }
            : new { disliked = previous, dislikes = Math.Max(0, comment.Dislikes + (previous ? -1 : 1)) });
    }
}
