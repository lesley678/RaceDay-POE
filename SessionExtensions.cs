namespace RaceDay.MVC.Services;
public static class SessionExtensions { public static bool IsLoggedIn(this ISession s)=>s.GetInt32("UserId").HasValue; public static bool IsRole(this ISession s,string role)=>string.Equals(s.GetString("UserRole"),role,StringComparison.OrdinalIgnoreCase); }
