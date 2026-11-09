using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SunshineAttractions.Data;

namespace SunshineAttractions.Controllers;

[Route("map")]
public sealed class MapController(MongoContext database, IConfiguration configuration) : Controller
{
    [HttpGet("maps")]
    public IActionResult Maps() => View();

    [HttpGet("magickingdom-maps")]
    public async Task<IActionResult> MagicKingdomMaps(CancellationToken cancellationToken)
    {
        ViewData["MapApiUrl"] = $"https://maps.googleapis.com/maps/api/js?key={configuration["GoogleMaps:ApiKey"]}&map_ids={configuration["GoogleMaps:MapId"]}&callback=initMap";
        return View(await database.Markers.Find(x => x.Park == "magic-kingdom").ToListAsync(cancellationToken));
    }
}
