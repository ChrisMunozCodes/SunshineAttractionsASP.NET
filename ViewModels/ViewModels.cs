using SunshineAttractions.Models;
using SunshineAttractions.Services;

namespace SunshineAttractions.ViewModels;

public sealed class PageViewModel
{
    public required Attraction Attraction { get; init; }
    public required IReadOnlyList<ReviewCard> Reviews { get; init; }
    public double AverageRating { get; init; }
    public bool HasReviewed { get; init; }
    public string Filter { get; init; } = "all";
}

public sealed class ParkSelectionViewModel
{
    public required ParkGuide Park { get; init; }
    public required IReadOnlyList<Attraction> Attractions { get; init; }
    public required IReadOnlyDictionary<string, double> Averages { get; init; }
}

public sealed class DayPlanViewModel
{
    public required IReadOnlyList<ParkGuide> Parks { get; init; }
    public required IReadOnlyList<Attraction> Rides { get; init; }
}
public sealed class ReviewCard
{
    public required Review Review { get; init; }
    public required User User { get; init; }
    public IReadOnlyList<CommentCard> Comments { get; init; } = [];
}
public sealed class CommentCard
{
    public required Comment Comment { get; init; }
    public required User User { get; init; }
}
