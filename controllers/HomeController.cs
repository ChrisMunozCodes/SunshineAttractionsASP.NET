using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Models;
using SunshineAttractions.Services;
using SunshineAttractions.ViewModels;

namespace SunshineAttractions.Controllers;

public sealed class HomeController(MongoContext database, CatalogueService catalogue) : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("disney-selection-homepage")]
    public IActionResult DisneySelectionHomepage() => View(catalogue.Parks.Where(park => park.Id != "seaworld-orlando").ToList());

    [HttpGet("plan")]
    public IActionResult Plan() => View(new DayPlanViewModel
    {
        Parks = catalogue.Parks,
        Rides = catalogue.Attractions.Where(attraction => attraction.Category == "Rides").ToList()
    });

    [HttpGet("disney-selection-magickingdom")]
    public Task<IActionResult> DisneySelectionMagicKingdom(CancellationToken cancellationToken) => ParkSelection("magic-kingdom", cancellationToken);

    [HttpGet("disney-selection/{parkId}")]
    public async Task<IActionResult> ParkSelection(string parkId, CancellationToken cancellationToken)
    {
        var park = catalogue.FindPark(parkId);
        if (park is null) return NotFound();

        var averages = (await database.Reviews.Find(Builders<Review>.Filter.Empty).ToListAsync(cancellationToken))
            .GroupBy(review => review.Page).ToDictionary(group => group.Key, group => group.Average(review => review.Rating));
        return View("DisneySelectionMagicKingdom", new ParkSelectionViewModel
        {
            Park = park,
            Attractions = catalogue.ForPark(park.Id),
            Averages = averages
        });
    }

    [HttpGet("page/{id}")]
    public async Task<IActionResult> Page(string id, string filter = "all", CancellationToken cancellationToken = default)
    {
        var attraction = catalogue.Find(id) ?? new Attraction(id, System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(id.Replace('-', ' ')), "Attractions", "/assets/image/Sunshine%20Attractions-logo/default.png", "Share your experience with other visitors.", []);
        var reviews = await database.Reviews.Find(x => x.Page == attraction.Id).SortByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        if (this.TryGetUserId(out var currentUser) && filter == "your") reviews = reviews.Where(x => x.UserId == currentUser).ToList();
        if (filter == "highest-rated") reviews = reviews.OrderByDescending(x => x.Rating).ToList();
        if (filter == "most-critical") reviews = reviews.OrderBy(x => x.Rating).ToList();
        var cards = await CreateCards(reviews, cancellationToken);
        var allForAverage = filter == "all" ? reviews : await database.Reviews.Find(x => x.Page == attraction.Id).ToListAsync(cancellationToken);
        return View(new PageViewModel
        {
            Attraction = attraction,
            Reviews = cards,
            AverageRating = allForAverage.Count == 0 ? 0 : allForAverage.Average(x => x.Rating),
            HasReviewed = this.TryGetUserId(out var userId) && allForAverage.Any(x => x.UserId == userId),
            Filter = filter
        });
    }

    [HttpGet("your-reviews")]
    public async Task<IActionResult> YourReviews(CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var id)) return Challenge();
        var reviews = await database.Reviews.Find(x => x.UserId == id).SortByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return View(await CreateCards(reviews, cancellationToken));
    }

    [HttpGet("your-comments")]
    public async Task<IActionResult> YourComments(CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var id)) return Challenge();
        var comments = await database.Comments.Find(x => x.UserId == id).SortByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        var user = await database.Users.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return View(comments.Select(comment => new CommentCard { Comment = comment, User = user! }).ToList());
    }

    [Route("Home/Error")]
    public IActionResult Error() => View();

    private async Task<IReadOnlyList<ReviewCard>> CreateCards(IReadOnlyList<Review> reviews, CancellationToken cancellationToken)
    {
        var userIds = reviews.Select(x => x.UserId).Distinct().ToList();
        var users = (await database.Users.Find(x => userIds.Contains(x.Id)).ToListAsync(cancellationToken)).ToDictionary(x => x.Id);
        var reviewIds = reviews.Select(x => x.Id).ToList();
        var comments = await database.Comments.Find(x => reviewIds.Contains(x.ReviewId)).SortBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        var commentUserIds = comments.Select(c => c.UserId).Distinct().ToList();
        var commentUsers = (await database.Users.Find(x => commentUserIds.Contains(x.Id)).ToListAsync(cancellationToken)).ToDictionary(x => x.Id);
        return reviews.Where(x => users.ContainsKey(x.UserId)).Select(review => new ReviewCard
        {
            Review = review,
            User = users[review.UserId],
            Comments = comments.Where(c => c.ReviewId == review.Id && commentUsers.ContainsKey(c.UserId)).Select(c => new CommentCard { Comment = c, User = commentUsers[c.UserId] }).ToList()
        }).ToList();
    }
}
