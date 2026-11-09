using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using SunshineAttractions.Data;
using SunshineAttractions.Services;

var builder = WebApplication.CreateBuilder(args);

// Preserve the original project's config/.env workflow without committing secrets.
var envFile = Path.Combine(builder.Environment.ContentRootPath, "config", ".env");
if (File.Exists(envFile))
{
    var values = new Dictionary<string, string?>();
    foreach (var rawLine in File.ReadLines(envFile))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#')) continue;
        var separator = line.IndexOf('=');
        if (separator <= 0) continue;
        var key = line[..separator].Trim().Replace("__", ":");
        var value = line[(separator + 1)..].Trim().Trim('"', '\'');
        values[key] = value;
    }
    builder.Configuration.AddInMemoryCollection(values);
}

// Avoid the Windows Event Log provider: it requires elevated permissions on many local machines.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<CatalogueService>();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseHttpsRedirection();
// Keep the original asset folders in place while the app is migrated to wwwroot later.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "public")),
    RequestPath = ""
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "views", "assets")),
    RequestPath = "/assets"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "dist")),
    RequestPath = "/css"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/wait-times", async (IHttpClientFactory clients, CancellationToken cancellationToken) =>
{
    using var request = new HttpRequestMessage(HttpMethod.Get, "https://queue-times.com/parks/6/queue_times.json");
    request.Headers.UserAgent.ParseAdd("SunshineAttractions/1.0 (+https://sunshineattractionsasp-net.onrender.com)");
    using var response = await clients.CreateClient().SendAsync(request, cancellationToken);
    return response.IsSuccessStatusCode
        ? Results.Content(await response.Content.ReadAsStringAsync(cancellationToken), "application/json")
        : Results.Problem("Unable to fetch wait times.", statusCode: 502);
});

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
