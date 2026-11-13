namespace SunshineAttractions.Services;

public sealed record ParkPlace(
    string Id,
    string ParkId,
    string Name,
    string Type,
    string Description,
    string[] Tags,
    double Latitude,
    double Longitude,
    string NavigationQuery,
    string SourceUrl);

public sealed class ParkFinderService
{
    // Curated visitor-support locations. Verify availability in the park's official app on arrival.
    public IReadOnlyList<ParkPlace> Places { get; } =
    [
        new("altitude-burgers", "seaworld-orlando", "Altitude Burgers", "food", "Quick-service burgers and fries near the coaster zone.", ["Quick service", "All-Day Dining"], 28.41045, -81.46255, "Altitude Burgers SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("voyagers-smokehouse", "seaworld-orlando", "Voyager's Smokehouse", "food", "Barbecue-style quick service with covered seating nearby.", ["Quick service", "All-Day Dining", "Covered seating"], 28.40945, -81.46065, "Voyager's Smokehouse SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("expedition-cafe", "seaworld-orlando", "Expedition Café", "food", "International quick-service choices by Antarctica.", ["Quick service", "All-Day Dining", "Indoor seating"], 28.41205, -81.46035, "Expedition Cafe SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("seafire-grill", "seaworld-orlando", "Seafire Grill", "food", "Quick-service grill with an indoor seating option.", ["Quick service", "All-Day Dining", "Indoor seating"], 28.40755, -81.46220, "Seafire Grill SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("waterway-grill", "seaworld-orlando", "Waterway Grill", "food", "Quick-service meal stop beside the lagoon.", ["Quick service", "All-Day Dining"], 28.40885, -81.46415, "Waterway Grill SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("sharks-underwater-grill", "seaworld-orlando", "Sharks Underwater Grill", "food", "Table-service dining beside Shark Encounter; ask at the host stand about same-day seating.", ["Table service", "Indoor seating", "Cool break"], 28.40970, -81.46395, "Sharks Underwater Grill SeaWorld Orlando", "https://seaworld.com/orlando/dining/"),
        new("first-aid-key-west", "seaworld-orlando", "First Aid — Key West", "first-aid", "Staffed First Aid behind Stingray Lagoon in the Key West area.", ["Nurses / EMTs", "Guest support"], 28.40870, -81.46465, "First Aid SeaWorld Orlando Key West", "https://seaworld.com/orlando/help/guests-with-disabilities/"),
        new("first-aid-sesame", "seaworld-orlando", "First Aid — Sesame Street Land", "first-aid", "Staffed First Aid near the Sesame Street Land restrooms.", ["Nurses / EMTs", "Guest support"], 28.41110, -81.45995, "First Aid SeaWorld Orlando Sesame Street", "https://seaworld.com/orlando/help/guests-with-disabilities/"),
        new("restroom-atlantis", "seaworld-orlando", "Family Restroom — Journey to Atlantis Plaza", "restroom", "Accessible companion/family restroom by the Journey to Atlantis plaza.", ["Accessible", "Family restroom"], 28.40940, -81.46210, "Journey to Atlantis SeaWorld Orlando", "https://seaworld.com/orlando/help/guests-with-disabilities/"),
        new("restroom-shark-encounter", "seaworld-orlando", "Family Restroom — Shark Encounter", "restroom", "Accessible companion/family restroom by the Shark Encounter exit.", ["Accessible", "Family restroom"], 28.40970, -81.46370, "Shark Encounter SeaWorld Orlando", "https://seaworld.com/orlando/help/guests-with-disabilities/"),
        new("restroom-sesame", "seaworld-orlando", "Family Restroom — Sesame Street Land", "restroom", "Accessible companion/family restroom by the Baby Care Center.", ["Accessible", "Family restroom", "Baby care"], 28.41100, -81.45980, "Sesame Street Land SeaWorld Orlando", "https://seaworld.com/orlando/help/guests-with-disabilities/"),
        new("main-lockers", "seaworld-orlando", "Main Entrance Lockers", "locker", "Daily rental lockers near the park entrance; small ride lockers are also available at several major coasters.", ["Daily rental", "Near entrance"], 28.41145, -81.46155, "SeaWorld Orlando main entrance lockers", "https://seaworld.com/orlando/faq/"),
        new("sesame-quiet-room", "seaworld-orlando", "Quiet Room — Sesame Street Land", "break", "A lower-stimulation break space inside the Child Care Facility.", ["Indoor", "Quiet break", "Family-friendly"], 28.41102, -81.45972, "Sesame Street Land SeaWorld Orlando", "https://seaworld.com/orlando/-/media/migrated-media/seaworld-orlando/files/pdf/swf_sensory-guide_2023.pdf")
    ];

    public IReadOnlyList<ParkPlace> ForPark(string parkId) => Places
        .Where(place => place.ParkId.Equals(parkId, StringComparison.OrdinalIgnoreCase))
        .ToList();
}
