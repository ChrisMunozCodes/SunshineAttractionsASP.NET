using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SunshineAttractions.Data;
using SunshineAttractions.Models;

namespace SunshineAttractions.Controllers;

[Route("")]
public sealed class AccountController(MongoContext database) : Controller
{
    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(input);
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var user = await database.Users.Find(x => x.Email == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(input);
        }
        await SignInAsync(user);
        return LocalRedirect(!string.IsNullOrWhiteSpace(input.ReturnUrl) && Url.IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl : "/profile");
    }

    [HttpGet("signup")]
    public IActionResult Signup() => View();

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(input);
        var email = input.Email.Trim().ToLowerInvariant();
        var userName = input.UserName.Trim();
        var existing = await database.Users.Find(x => x.Email == email || x.UserName == userName).AnyAsync(cancellationToken);
        if (existing)
        {
            ModelState.AddModelError("", "An account with that email address or username already exists.");
            return View(input);
        }
        var user = new User { Email = email, UserName = userName, PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password) };
        await database.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
        await SignInAsync(user);
        return Redirect("/profile");
    }

    [Authorize, HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [Authorize, HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        if (!this.TryGetUserId(out var id)) return Challenge();
        var user = await database.Users.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return user is null ? Challenge() : View(user);
    }

    [Authorize, HttpPost("updateProfilePicture")]
    public IActionResult UpdateProfilePicture()
    {
        TempData["Error"] = "Image uploads are not currently supported.";
        return Redirect("/profile");
    }

    private async Task SignInAsync(User user) => await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Name, user.UserName)], CookieAuthenticationDefaults.AuthenticationScheme)));

}

public sealed class LoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public sealed class SignupInput
{
    [Required, StringLength(24, MinimumLength = 2)] public string UserName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}
