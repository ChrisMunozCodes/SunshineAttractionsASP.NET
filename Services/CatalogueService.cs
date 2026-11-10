namespace SunshineAttractions.Services;

public sealed record Attraction(string Id, string Name, string Category, string Image, string Description, string[] Tags, string? WaitKey = null, string ParkId = "magic-kingdom");

public sealed record ParkGuide(
    string Id,
    string Name,
    string Tagline,
    string Description,
    string Image,
    string MapRoute,
    int QueueTimesParkId,
    double Latitude,
    double Longitude);

public sealed class CatalogueService
{
    private const string CastleImage = "/assets/image/Sunshine%20Attractions-logo/8731_castle_landing_page_image_nu_2a8f8e6a-a1f9-45fd-8eca-596c92d04a7c.jpeg";
    private const string CoasterImage = "/assets/image/Sunshine%20Attractions-logo/rollercoaster.png";
    private const string SpotlightImage = "/assets/image/Sunshine%20Attractions-logo/spotlight.png";

    public IReadOnlyList<ParkGuide> Parks { get; } =
    [
        new("magic-kingdom", "Magic Kingdom", "Classic magic", "Castles, classic attractions, and fireworks from morning through night.", CastleImage, "/map/magickingdom-maps", 6, 28.41874, -81.58121),
        new("epcot", "EPCOT", "Curiosity comes alive", "Big ideas, global flavors, and out-of-this-world adventures.", CoasterImage, "/map/epcot", 5, 28.37470, -81.54940),
        new("hollywood-studios", "Hollywood Studios", "Step into the story", "Star Wars adventures, Pixar play, and show-stopping thrills.", SpotlightImage, "/map/hollywood-studios", 7, 28.35750, -81.55890),
        new("animal-kingdom", "Animal Kingdom", "Wild by nature", "Immersive lands, living creatures, and expeditions after dark.", "/assets/image/Sunshine%20Attractions-logo/fast-food.png", "/map/animal-kingdom", 8, 28.35530, -81.59020)
    ];

    // IDs intentionally match the Node/Mongoose data so existing reviews remain associated.
    public IReadOnlyList<Attraction> Attractions { get; } =
    [
        new("space-mountain", "Space Mountain", "Rides", "/assets/image/Sunshine%20Attractions-logo/space-mountain.jpeg", "An indoor space-themed roller coaster with twists, turns, and sudden drops.", ["dark ride", "rollercoaster", "thrill"], "space-wait"),
        new("thunder-mountain", "Thunder Mountain", "Rides", "/assets/image/Sunshine%20Attractions-logo/big-thunder-mountain.jpeg", "A wild mine-train adventure through the Old West.", ["family thrills", "rollercoaster"], "thunder-wait"),
        new("haunted-mansion", "Haunted Mansion", "Rides", "/assets/image/Sunshine%20Attractions-logo/haunted-mansion.jpg", "A classic dark ride through a ghostly estate.", ["dark ride", "haunted", "immersive"], "haunted-wait"),
        new("seven-dwarfs-mine-train", "Seven Dwarfs Mine Train", "Rides", "/assets/image/Sunshine%20Attractions-logo/mine-train.jpeg", "A family coaster through the Seven Dwarfs' diamond mine.", ["family", "rollercoaster"], "dwarfs-wait"),
        new("pirates-of-the-caribbean", "Pirates of the Caribbean", "Rides", "/assets/image/Sunshine%20Attractions-logo/Pirates_of_the_Caribbean_at_Walt_Disney_World_January_2012.jpeg", "A swashbuckling boat ride through pirate adventures.", ["dark ride", "adventure"], "pirates-wait"),
        new("jungle-cruise", "Jungle Cruise", "Rides", "/assets/image/Sunshine%20Attractions-logo/Hong_Kong_Disneyland_(10098310885).jpeg", "A scenic river tour with famous skipper jokes.", ["boat ride", "humor"], "jungle-wait"),
        new("dumbo-the-flying-elephant", "Dumbo the Flying Elephant", "Rides", "/assets/image/Sunshine%20Attractions-logo/Dumbo_the_Flying_Elephant_Close-up.jpeg", "A timeless spinning flight above Fantasyland.", ["family", "classic"], "dumbo-wait"),
        new("tomorrowland-transit-authority-peoplemover", "Tomorrowland PeopleMover", "Rides", "/assets/image/Sunshine%20Attractions-logo/PeopleMover_Sign_(28581641953).jpeg", "A relaxing elevated tour of Tomorrowland.", ["scenic", "futuristic"], "peoplemover-wait"),
        new("its-a-small-world", "It's a Small World", "Rides", "/assets/image/Sunshine%20Attractions-logo/640px-It's_a_Small_World_-_Magic_Kingdom4.jpeg", "A cheerful voyage celebrating cultures from around the world.", ["classic", "boat ride"], "smallWorld-wait"),
        new("prince-charming-regal-carrousel", "Prince Charming Regal Carrousel", "Rides", "/assets/image/Sunshine%20Attractions-logo/Cinderella's_Golden_Carrousel_TDL.jpeg", "A classic carousel in the heart of Fantasyland.", ["classic", "carousel"], "carrousel-wait"),
        new("peter-pans-flight", "Peter Pan's Flight", "Rides", "/assets/image/Sunshine%20Attractions-logo/peter-pan.jpeg", "Fly over London and Neverland in a pirate ship.", ["family", "fantasy"], "peterPan-wait"),
        new("under-the-sea-journey-of-the-little-mermaid", "Under the Sea: Journey of the Little Mermaid", "Rides", "/assets/image/Sunshine%20Attractions-logo/The_Little_Mermaid_~_Ariel's_Undersea_Adventure_(view_in_2023).jpeg", "A musical undersea journey with Ariel and friends.", ["family", "musical"], "mermaid-wait"),
        new("the-barnstormer", "The Barnstormer", "Rides", "/assets/image/Sunshine%20Attractions-logo/Barnstormer_(Magic_Kingdom)_1.jpeg", "A kid-friendly coaster with Goofy.", ["family", "rollercoaster"], "barnstormer-wait"),
        new("the-magic-carpets-of-aladdin", "The Magic Carpets of Aladdin", "Rides", "/assets/image/Sunshine%20Attractions-logo/640px-Tokyo_DisneySea_The_Magic_Carpets_of_Aladdin_201306.jpeg", "Soar over Agrabah on a magic carpet.", ["family", "spinner"], "aladdin-wait"),
        new("cinderellas-royal-table", "Cinderella's Royal Table", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "A royal character dining experience inside Cinderella Castle.", ["character dining", "fine dining"]),
        new("be-our-guest-restaurant", "Be Our Guest Restaurant", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "French-inspired dining in Beast's enchanted castle.", ["Beauty and the Beast", "fine dining"]),
        new("aloha-isle", "Aloha Isle", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "Home of the famous Dole Whip.", ["snacks", "Dole Whip"]),
        new("caseys-corner", "Casey's Corner", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "Classic hot dogs and Americana on Main Street.", ["quick service", "baseball"]),
        new("columbia-harbour-house", "Columbia Harbour House", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "Seafood and comfort food in Liberty Square.", ["seafood", "nautical"]),
        new("cosmic-rays-starlight-cafe", "Cosmic Ray's Starlight Cafe", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "Burgers and a space-age floor show in Tomorrowland.", ["quick service", "space"]),
        new("happily-ever-after", "Happily Ever After", "Shows", "/assets/image/Sunshine%20Attractions-logo/disney-magickingdom-shows.jpg", "A nighttime fireworks spectacular.", ["fireworks", "nighttime"]),
        new("festival-of-fantasy-parade", "Festival of Fantasy Parade", "Shows", "/assets/image/Sunshine%20Attractions-logo/disney-magickingdom-shows.jpg", "A colorful daytime Disney parade.", ["parade", "characters"]),
        new("enchanted-tiki-room", "Enchanted Tiki Room", "Shows", "/assets/image/Sunshine%20Attractions-logo/disney-magickingdom-shows.jpg", "A classic musical show with singing birds.", ["classic", "musical"]),

        // EPCOT
        new("guardians-of-the-galaxy-cosmic-rewind", "Guardians of the Galaxy: Cosmic Rewind", "Rides", CoasterImage, "A high-energy indoor coaster that spins you through space with the Guardians.", ["rollercoaster", "thrill", "indoor"], null, "epcot"),
        new("test-track", "Test Track", "Rides", CoasterImage, "Design your own virtual concept car and put it through its paces.", ["thrill", "speed", "family"], null, "epcot"),
        new("soarin-around-the-world", "Soarin' Around the World", "Rides", SpotlightImage, "A breathtaking hang-gliding tour over some of the world's most beautiful places.", ["scenic", "family", "gentle"], null, "epcot"),
        new("remys-ratatouille-adventure", "Remy's Ratatouille Adventure", "Rides", "/assets/image/Sunshine%20Attractions-logo/rollercoaster.png", "Shrink to the size of a rat for a 4D chase through Gusteau's kitchen.", ["dark ride", "family", "immersive"], null, "epcot"),
        new("frozen-ever-after", "Frozen Ever After", "Rides", CastleImage, "Set sail through Arendelle with Anna, Elsa, and all your favorite Frozen friends.", ["boat ride", "family", "musical"], null, "epcot"),
        new("spaceship-earth", "Spaceship Earth", "Rides", SpotlightImage, "Travel through the history of human communication inside EPCOT's iconic geosphere.", ["classic", "educational", "gentle"], null, "epcot"),
        new("living-with-the-land", "Living with the Land", "Rides", "/assets/image/Sunshine%20Attractions-logo/fast-food.png", "A gentle boat tour of remarkable greenhouses and growing techniques.", ["boat ride", "educational", "gentle"], null, "epcot"),
        new("mission-space", "Mission: SPACE", "Rides", CoasterImage, "Train for a mission beyond Earth in an intense or family-friendly simulator.", ["simulator", "space", "thrill"], null, "epcot"),
        new("space-220", "Space 220 Restaurant", "Dining", "/assets/image/Sunshine%20Attractions-logo/black.jpeg", "Dine 220 miles above Earth in a celestial space-station setting.", ["table service", "space", "immersive"], null, "epcot"),
        new("luminous-the-symphony-of-us", "Luminous: The Symphony of Us", "Shows", SpotlightImage, "A nighttime spectacular of fountains, music, lights, and fireworks around World Showcase Lagoon.", ["fireworks", "nighttime"], null, "epcot"),

        // Disney's Hollywood Studios
        new("star-wars-rise-of-the-resistance", "Star Wars: Rise of the Resistance", "Rides", SpotlightImage, "Join the Resistance in a cinematic battle against the First Order.", ["dark ride", "thrill", "immersive"], null, "hollywood-studios"),
        new("mickeys-and-minnies-runaway-railway", "Mickey & Minnie's Runaway Railway", "Rides", CastleImage, "Board a runaway cartoon train through a surprise-filled Mickey short.", ["dark ride", "family", "immersive"], null, "hollywood-studios"),
        new("slinky-dog-dash", "Slinky Dog Dash", "Rides", CoasterImage, "A family coaster that zips and bounces across Andy's backyard.", ["rollercoaster", "family", "outdoor"], null, "hollywood-studios"),
        new("tower-of-terror", "The Twilight Zone Tower of Terror", "Rides", SpotlightImage, "Check in to the Hollywood Tower Hotel for a truly unpredictable drop sequence.", ["drop ride", "thrill", "haunted"], null, "hollywood-studios"),
        new("rock-n-roller-coaster", "Rock 'n' Roller Coaster", "Rides", CoasterImage, "Launch through Los Angeles on a high-speed limousine ride with a rock soundtrack.", ["rollercoaster", "thrill", "indoor"], null, "hollywood-studios"),
        new("millennium-falcon-smugglers-run", "Millennium Falcon: Smugglers Run", "Rides", SpotlightImage, "Pilot the fastest hunk of junk in the galaxy on a mission for Hondo Ohnaka.", ["simulator", "Star Wars", "interactive"], null, "hollywood-studios"),
        new("toy-story-mania", "Toy Story Mania!", "Rides", CastleImage, "Compete in a 4D carnival midway game hosted by the Toy Story gang.", ["interactive", "family", "indoor"], null, "hollywood-studios"),
        new("beauty-and-the-beast-live-on-stage", "Beauty and the Beast – Live on Stage", "Shows", SpotlightImage, "A Broadway-style retelling of the beloved Disney classic.", ["musical", "live show", "family"], null, "hollywood-studios"),
        new("roundup-rodeo-bbq", "Roundup Rodeo BBQ", "Dining", "/assets/image/Sunshine%20Attractions-logo/fast-food.png", "Enjoy a playful family-style barbecue in Andy's backyard.", ["table service", "Toy Story", "barbecue"], null, "hollywood-studios"),

        // Disney's Animal Kingdom
        new("avatar-flight-of-passage", "Avatar Flight of Passage", "Rides", SpotlightImage, "Fly on the back of a banshee across the world of Pandora in a breathtaking 3D adventure.", ["simulator", "thrill", "immersive"], null, "animal-kingdom"),
        new("navi-river-journey", "Na'vi River Journey", "Rides", CastleImage, "Glide through Pandora's bioluminescent rainforest on a peaceful river expedition.", ["boat ride", "gentle", "immersive"], null, "animal-kingdom"),
        new("expedition-everest", "Expedition Everest", "Rides", CoasterImage, "Race through the Himalayas on a runaway train while avoiding the legendary Yeti.", ["rollercoaster", "thrill", "adventure"], null, "animal-kingdom"),
        new("kilimanjaro-safaris", "Kilimanjaro Safaris", "Rides", "/assets/image/Sunshine%20Attractions-logo/fast-food.png", "Take an open-air expedition across an African savanna filled with live animals.", ["animals", "family", "outdoor"], null, "animal-kingdom"),
        new("kali-river-rapids", "Kali River Rapids", "Rides", SpotlightImage, "Brave churning whitewater through an Asian jungle river.", ["water ride", "thrill", "outdoor"], null, "animal-kingdom"),
        new("festival-of-the-lion-king", "Festival of the Lion King", "Shows", SpotlightImage, "Celebrate Simba's journey with acrobatics, music, and larger-than-life puppetry.", ["musical", "live show", "family"], null, "animal-kingdom"),
        new("finding-nemo-the-big-blue-and-beyond", "Finding Nemo: The Big Blue... and Beyond!", "Shows", CastleImage, "A vibrant stage musical that dives into the story of Nemo and friends.", ["musical", "live show", "family"], null, "animal-kingdom"),
        new("yak-and-yeti-restaurant", "Yak & Yeti Restaurant", "Dining", "/assets/image/Sunshine%20Attractions-logo/fast-food.png", "Pan-Asian favorites served in a richly detailed Himalayan village.", ["table service", "Asian", "adventure"], null, "animal-kingdom"),
        new("tree-of-life-awakenings", "Tree of Life Awakenings", "Shows", SpotlightImage, "Watch the park's iconic tree transform with colorful nighttime projections.", ["nighttime", "projections", "family"], null, "animal-kingdom")
    ];

    public Attraction? Find(string id) => Attractions.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    public ParkGuide? FindPark(string id) => Parks.FirstOrDefault(park => park.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<Attraction> ForPark(string parkId) => Attractions.Where(item => item.ParkId.Equals(parkId, StringComparison.OrdinalIgnoreCase)).ToList();
}
