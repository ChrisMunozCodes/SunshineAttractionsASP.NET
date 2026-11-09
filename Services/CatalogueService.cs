namespace SunshineAttractions.Services;

public sealed record Attraction(string Id, string Name, string Category, string Image, string Description, string[] Tags, string? WaitKey = null);

public sealed class CatalogueService
{
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
        new("enchanted-tiki-room", "Enchanted Tiki Room", "Shows", "/assets/image/Sunshine%20Attractions-logo/disney-magickingdom-shows.jpg", "A classic musical show with singing birds.", ["classic", "musical"])
    ];

    public Attraction? Find(string id) => Attractions.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
