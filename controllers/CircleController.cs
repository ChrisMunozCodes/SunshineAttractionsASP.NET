using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Models;
using SunshineAttractions.Services;

namespace SunshineAttractions.Controllers;

[Authorize]
[Route("circle")]
public sealed class CircleController(MongoContext database, CatalogueService catalogue, IConfiguration configuration) : Controller
{
    private static readonly TimeSpan CircleLifetime = TimeSpan.FromHours(12);
    private static readonly TimeSpan LocationFreshness = TimeSpan.FromMinutes(2);

    [HttpGet("")]
    public IActionResult Index() => View(catalogue.Parks);

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateCircleInput input, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId)) return Challenge();
        var park = catalogue.FindPark(input.ParkId ?? "");
        var name = (input.Name ?? "").Trim();
        if (park is null || name.Length is < 2 or > 40)
        {
            TempData["Error"] = "Choose a park and give your Circle a name between 2 and 40 characters.";
            return RedirectToAction(nameof(Index));
        }

        var displayName = User.Identity?.Name ?? "Park guest";
        var circle = new ParkCircle
        {
            OwnerId = userId,
            Name = name,
            ParkId = park.Id,
            InviteCode = await CreateInviteCode(cancellationToken),
            ExpiresAt = DateTime.UtcNow.Add(CircleLifetime),
            Members = [new ParkCircleMember { UserId = userId, DisplayName = displayName }]
        };
        await database.ParkCircles.InsertOneAsync(circle, cancellationToken: cancellationToken);
        return RedirectToAction(nameof(Room), new { id = circle.Id.ToString() });
    }

    [HttpGet("join/{inviteCode}")]
    public async Task<IActionResult> Join(string inviteCode, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId)) return Challenge();
        var now = DateTime.UtcNow;
        var circle = await database.ParkCircles.Find(circle => circle.InviteCode == inviteCode && circle.ExpiresAt > now).FirstOrDefaultAsync(cancellationToken);
        if (circle is null)
        {
            TempData["Error"] = "That Park Circle is no longer active. Ask the organizer for a fresh invite.";
            return RedirectToAction(nameof(Index));
        }

        if (!circle.Members.Any(member => member.UserId == userId))
        {
            var member = new ParkCircleMember { UserId = userId, DisplayName = User.Identity?.Name ?? "Park guest" };
            await database.ParkCircles.UpdateOneAsync(
                item => item.Id == circle.Id && !item.Members.Any(existing => existing.UserId == userId),
                Builders<ParkCircle>.Update.Push(item => item.Members, member), cancellationToken: cancellationToken);
        }
        return RedirectToAction(nameof(Room), new { id = circle.Id.ToString() });
    }

    [HttpGet("room/{id}")]
    public async Task<IActionResult> Room(string id, CancellationToken cancellationToken)
    {
        var circle = await FindMemberCircle(id, cancellationToken);
        if (circle is null) return RedirectToAction(nameof(Index));
        var park = catalogue.FindPark(circle.ParkId);
        if (park is null) return RedirectToAction(nameof(Index));
        ViewData["Park"] = park;
        ViewData["MapApiUrl"] = $"https://maps.googleapis.com/maps/api/js?key={configuration["GoogleMaps:ApiKey"]}&map_ids={configuration["GoogleMaps:MapId"]}&callback=initCircleMap";
        return View(circle);
    }

    [HttpGet("api/{id}")]
    public async Task<IActionResult> State(string id, CancellationToken cancellationToken)
    {
        var circle = await FindMemberCircle(id, cancellationToken);
        if (circle is null) return NotFound();
        var now = DateTime.UtcNow;
        return Json(new
        {
            name = circle.Name,
            parkId = circle.ParkId,
            expiresAt = circle.ExpiresAt,
            members = circle.Members.Select(member => new
            {
                id = member.UserId.ToString(),
                name = member.DisplayName,
                isSharing = member.IsSharingLocation,
                isFresh = member.IsSharingLocation && member.LocationUpdatedAt is not null && now - member.LocationUpdatedAt <= LocationFreshness,
                latitude = member.IsSharingLocation && member.LocationUpdatedAt is not null && now - member.LocationUpdatedAt <= LocationFreshness ? member.Latitude : null,
                longitude = member.IsSharingLocation && member.LocationUpdatedAt is not null && now - member.LocationUpdatedAt <= LocationFreshness ? member.Longitude : null,
                updatedAt = member.LocationUpdatedAt
            })
        });
    }

    [HttpPost("api/{id}/location")]
    public async Task<IActionResult> UpdateLocation(string id, [FromBody] CircleLocationInput input, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId)) return Unauthorized();
        var circle = await FindMemberCircle(id, cancellationToken);
        if (circle is null) return NotFound();

        var filter = Builders<ParkCircle>.Filter.And(
            Builders<ParkCircle>.Filter.Eq(circle => circle.Id, circle.Id),
            Builders<ParkCircle>.Filter.ElemMatch(circle => circle.Members, member => member.UserId == userId));
        var update = Builders<ParkCircle>.Update;
        if (!input.IsSharing)
        {
            await database.ParkCircles.UpdateOneAsync(filter, update.Combine(
                update.Set("members.$.sharing", false),
                update.Set("members.$.latitude", BsonNull.Value),
                update.Set("members.$.longitude", BsonNull.Value),
                update.Set("members.$.locationUpdatedAt", BsonNull.Value)), cancellationToken: cancellationToken);
            return Ok();
        }

        if (input.Latitude is null || input.Longitude is null || input.Latitude is < -90 or > 90 || input.Longitude is < -180 or > 180) return BadRequest();
        await database.ParkCircles.UpdateOneAsync(filter, update.Combine(
            update.Set("members.$.sharing", true),
            update.Set("members.$.latitude", input.Latitude),
            update.Set("members.$.longitude", input.Longitude),
            update.Set("members.$.locationUpdatedAt", DateTime.UtcNow)), cancellationToken: cancellationToken);
        return Ok();
    }

    [HttpPost("api/{id}/leave")]
    public async Task<IActionResult> Leave(string id, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId)) return Unauthorized();
        if (!ObjectId.TryParse(id, out var circleId)) return NotFound();
        await database.ParkCircles.UpdateOneAsync(
            circle => circle.Id == circleId,
            Builders<ParkCircle>.Update.PullFilter(circle => circle.Members, member => member.UserId == userId), cancellationToken: cancellationToken);
        return Ok();
    }

    private async Task<ParkCircle?> FindMemberCircle(string id, CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var userId) || !ObjectId.TryParse(id, out var circleId)) return null;
        return await database.ParkCircles.Find(circle => circle.Id == circleId && circle.ExpiresAt > DateTime.UtcNow && circle.Members.Any(member => member.UserId == userId)).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<string> CreateInviteCode(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToLowerInvariant();
            if (!await database.ParkCircles.Find(circle => circle.InviteCode == code).AnyAsync(cancellationToken)) return code;
        }
        throw new InvalidOperationException("Unable to create a unique Park Circle invite.");
    }
}

public sealed class CreateCircleInput
{
    public string? Name { get; set; }
    public string? ParkId { get; set; }
}

public sealed class CircleLocationInput
{
    public bool IsSharing { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
