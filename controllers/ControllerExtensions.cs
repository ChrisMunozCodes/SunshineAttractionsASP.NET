using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace SunshineAttractions.Controllers;

public static class ControllerExtensions
{
    public static bool TryGetUserId(this Controller controller, out ObjectId id) =>
        ObjectId.TryParse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier), out id);
}
