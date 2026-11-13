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
    private const string MagicKingdomDining = "https://disneyworld.disney.go.com/dining/magic-kingdom/";
    private const string EpcotDining = "https://disneyworld.disney.go.com/dining/epcot/";
    private const string HollywoodStudiosDining = "https://disneyworld.disney.go.com/dining/hollywood-studios/";
    private const string AnimalKingdomDining = "https://disneyworld.disney.go.com/dining/animal-kingdom/";

    // Curated visitor-support locations. Verify availability in the park's official app on arrival.
    public IReadOnlyList<ParkPlace> Places { get; } =
    [
        // Magic Kingdom
        new("caseys-corner-finder", "magic-kingdom", "Casey's Corner", "food", "Classic quick-service hot dogs on Main Street, U.S.A.", ["Quick service", "Main Street"], 28.41800, -81.58135, "Casey's Corner Magic Kingdom", MagicKingdomDining),
        new("cosmic-rays-finder", "magic-kingdom", "Cosmic Ray's Starlight Café", "food", "Burgers and quick-service favorites in Tomorrowland.", ["Quick service", "Indoor seating"], 28.41895, -81.57765, "Cosmic Ray's Starlight Cafe Magic Kingdom", MagicKingdomDining),
        new("columbia-harbour-house-finder", "magic-kingdom", "Columbia Harbour House", "food", "Quick-service seafood and comfort food in Liberty Square.", ["Quick service", "Indoor seating"], 28.41958, -81.58318, "Columbia Harbour House Magic Kingdom", MagicKingdomDining),
        new("pecos-bill-finder", "magic-kingdom", "Pecos Bill Tall Tale Inn and Café", "food", "Southwest-inspired quick service near Frontierland.", ["Quick service", "Indoor seating"], 28.41735, -81.58420, "Pecos Bill Magic Kingdom", MagicKingdomDining),
        new("mk-first-aid", "magic-kingdom", "First Aid", "first-aid", "First Aid is located near the park entrance on Main Street, U.S.A.", ["Guest support", "Near entrance"], 28.41685, -81.58142, "Magic Kingdom First Aid", "https://disneyworld.disney.go.com/guest-services/first-aid/"),
        new("mk-restroom-tomorrowland", "magic-kingdom", "Restrooms — Tomorrowland", "restroom", "Restrooms near the Tomorrowland attractions area.", ["Accessible", "Family-friendly"], 28.41815, -81.57792, "Tomorrowland Restrooms Magic Kingdom", "https://disneyworld.disney.go.com/guest-services/restrooms/"),
        new("mk-lockers", "magic-kingdom", "Lockers — Main Street", "locker", "Rental lockers near the park entrance for items you do not want to carry all day.", ["Near entrance", "Rental"], 28.41672, -81.58125, "Magic Kingdom Lockers", "https://disneyworld.disney.go.com/guest-services/locker-rentals/"),

        // EPCOT
        new("sunshine-seasons-finder", "epcot", "Sunshine Seasons", "food", "Quick-service meals inside The Land pavilion.", ["Quick service", "Indoor seating"], 28.37155, -81.55225, "Sunshine Seasons EPCOT", EpcotDining),
        new("connections-eatery-finder", "epcot", "Connections Eatery", "food", "Central quick-service stop near World Celebration.", ["Quick service", "Indoor seating"], 28.37488, -81.54926, "Connections Eatery EPCOT", EpcotDining),
        new("regal-eagle-finder", "epcot", "Regal Eagle Smokehouse", "food", "American barbecue quick service in the American Adventure pavilion.", ["Quick service", "Barbecue"], 28.37152, -81.55288, "Regal Eagle Smokehouse EPCOT", EpcotDining),
        new("la-cantina-finder", "epcot", "La Cantina de San Angel", "food", "Mexican quick service along World Showcase Lagoon.", ["Quick service", "Mexican"], 28.37178, -81.55203, "La Cantina de San Angel EPCOT", EpcotDining),
        new("epcot-first-aid", "epcot", "First Aid", "first-aid", "First Aid near the front of the park in World Celebration.", ["Guest support", "Near entrance"], 28.37515, -81.55012, "EPCOT First Aid", "https://disneyworld.disney.go.com/guest-services/first-aid/"),
        new("epcot-restroom-land", "epcot", "Restrooms — The Land", "restroom", "Accessible restrooms inside The Land pavilion.", ["Accessible", "Indoor"], 28.37148, -81.55242, "The Land Restrooms EPCOT", "https://disneyworld.disney.go.com/guest-services/restrooms/"),
        new("epcot-lockers", "epcot", "Lockers — Main Entrance", "locker", "Rental lockers near the EPCOT entrance.", ["Near entrance", "Rental"], 28.37548, -81.54973, "EPCOT Lockers", "https://disneyworld.disney.go.com/guest-services/locker-rentals/"),

        // Disney's Hollywood Studios
        new("docking-bay-7-finder", "hollywood-studios", "Docking Bay 7 Food and Cargo", "food", "Star Wars: Galaxy's Edge quick-service meals.", ["Quick service", "Galaxy's Edge"], 28.35362, -81.56088, "Docking Bay 7 Hollywood Studios", HollywoodStudiosDining),
        new("backlot-express-finder", "hollywood-studios", "Backlot Express", "food", "Quick-service burgers and sandwiches near Echo Lake.", ["Quick service", "Indoor seating"], 28.35522, -81.56003, "Backlot Express Hollywood Studios", HollywoodStudiosDining),
        new("abc-commissary-finder", "hollywood-studios", "ABC Commissary", "food", "Indoor quick-service meals on Commissary Lane.", ["Quick service", "Indoor seating"], 28.35663, -81.55985, "ABC Commissary Hollywood Studios", HollywoodStudiosDining),
        new("woodys-lunch-box-finder", "hollywood-studios", "Woody's Lunch Box", "food", "Toy Story Land quick-service sandwiches and bowls.", ["Quick service", "Toy Story Land"], 28.35584, -81.56312, "Woody's Lunch Box Hollywood Studios", HollywoodStudiosDining),
        new("hs-first-aid", "hollywood-studios", "First Aid", "first-aid", "First Aid near the front of the park on Hollywood Boulevard.", ["Guest support", "Near entrance"], 28.35783, -81.55800, "Hollywood Studios First Aid", "https://disneyworld.disney.go.com/guest-services/first-aid/"),
        new("hs-restroom-echo-lake", "hollywood-studios", "Restrooms — Echo Lake", "restroom", "Accessible restrooms near Echo Lake.", ["Accessible"], 28.35564, -81.55952, "Echo Lake Restrooms Hollywood Studios", "https://disneyworld.disney.go.com/guest-services/restrooms/"),
        new("hs-lockers", "hollywood-studios", "Lockers — Main Entrance", "locker", "Rental lockers near the Hollywood Studios entrance.", ["Near entrance", "Rental"], 28.35798, -81.55828, "Hollywood Studios Lockers", "https://disneyworld.disney.go.com/guest-services/locker-rentals/"),

        // Disney's Animal Kingdom
        new("satuli-canteen-finder", "animal-kingdom", "Satu'li Canteen", "food", "Pandora quick-service bowls and family-friendly meals.", ["Quick service", "Pandora", "Indoor seating"], 28.35445, -81.59233, "Satu'li Canteen Animal Kingdom", AnimalKingdomDining),
        new("flame-tree-finder", "animal-kingdom", "Flame Tree Barbecue", "food", "Quick-service barbecue with shaded outdoor seating.", ["Quick service", "Barbecue", "Outdoor seating"], 28.35635, -81.58970, "Flame Tree Barbecue Animal Kingdom", AnimalKingdomDining),
        new("harambe-market-finder", "animal-kingdom", "Harambe Market", "food", "African-inspired quick-service market in Harambe.", ["Quick service", "Harambe"], 28.35694, -81.59115, "Harambe Market Animal Kingdom", AnimalKingdomDining),
        new("yak-yeti-local-foods-finder", "animal-kingdom", "Yak & Yeti Local Food Cafes", "food", "Asian-inspired quick-service meals in Asia.", ["Quick service", "Asian"], 28.35877, -81.58725, "Yak and Yeti Local Food Cafes Animal Kingdom", AnimalKingdomDining),
        new("ak-first-aid", "animal-kingdom", "First Aid", "first-aid", "First Aid near the park entrance on Discovery Island.", ["Guest support", "Near entrance"], 28.35455, -81.59038, "Animal Kingdom First Aid", "https://disneyworld.disney.go.com/guest-services/first-aid/"),
        new("ak-restroom-pandora", "animal-kingdom", "Restrooms — Pandora", "restroom", "Accessible restrooms in Pandora — The World of Avatar.", ["Accessible"], 28.35405, -81.59245, "Pandora Restrooms Animal Kingdom", "https://disneyworld.disney.go.com/guest-services/restrooms/"),
        new("ak-lockers", "animal-kingdom", "Lockers — Main Entrance", "locker", "Rental lockers near the Animal Kingdom entrance.", ["Near entrance", "Rental"], 28.35438, -81.59008, "Animal Kingdom Lockers", "https://disneyworld.disney.go.com/guest-services/locker-rentals/"),

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
