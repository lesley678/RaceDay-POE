
using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Security;

public static class SessionHelper
{
    public static int? GetUserId(HttpContext context)
    {
        return context.Session.GetInt32("UserId");
    }

    public static string? GetUserRole(HttpContext context)
    {
        return context.Session.GetString("UserRole");
    }

    public static bool IsLoggedIn(HttpContext context)
    {
        return GetUserId(context).HasValue;
    }

    public static bool IsOrganiser(HttpContext context)
    {
        return IsLoggedIn(context) &&
               string.Equals(
                   GetUserRole(context),
                   "Organiser",
                   StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsParticipant(HttpContext context)
    {
        return IsLoggedIn(context) &&
               string.Equals(
                   GetUserRole(context),
                   "Participant",
                   StringComparison.OrdinalIgnoreCase);
    }
}
