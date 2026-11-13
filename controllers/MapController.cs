using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Services;

namespace SunshineAttractions.Controllers;

[Route("map")]
public sealed class MapController(MongoContext database, IConfiguration configuration, ILogger<MapController> logger, CatalogueService catalogue, ParkFinderService parkFinder) : Controller
{
    [HttpGet("maps")]
    public IActionResult Maps() => View(catalogue.Parks);

    [HttpGet("magickingdom-maps")]
    public async Task<IActionResult> MagicKingdomMaps(CancellationToken cancellationToken)
    {
        ViewData["MapApiUrl"] = $"https://maps.googleapis.com/maps/api/js?key={configuration["GoogleMaps:ApiKey"]}&map_ids={configuration["GoogleMaps:MapId"]}&callback=initMap";
        ViewData["ParkPlaces"] = parkFinder.ForPark("magic-kingdom");
        try
        {
            return View(await database.Markers.Find(x => x.Park == "magic-kingdom").ToListAsync(cancellationToken));
        }
        catch (MongoException exception)
        {
            // The browser map includes a maintained fallback ride list. Keep the live planner
            // available if an external database temporarily cannot be reached.
            logger.LogWarning(exception, "Magic Kingdom marker data is unavailable; using browser fallback markers.");
            ViewData["MarkerDataUnavailable"] = true;
            return View(Array.Empty<SunshineAttractions.Models.Marker>());
        }
    }

    [HttpGet("{parkId}")]
    public IActionResult ParkMap(string parkId)
    {
        if (parkId.Equals("magic-kingdom", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(MagicKingdomMaps));

        var park = catalogue.FindPark(parkId);
        if (park is null) return NotFound();

        ViewData["MapApiUrl"] = $"https://maps.googleapis.com/maps/api/js?key={configuration["GoogleMaps:ApiKey"]}&map_ids={configuration["GoogleMaps:MapId"]}&callback=initParkPlannerMap";
        ViewData["ParkPlaces"] = parkFinder.ForPark(park.Id);
        return View("ParkMap", park);
    }
}
