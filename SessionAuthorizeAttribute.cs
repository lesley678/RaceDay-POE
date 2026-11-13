
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute : Attribute,
    IAsyncAuthorizationFilter
{
    private readonly string? _requiredRole;

    public SessionAuthorizeAttribute()
    {
    }

    public SessionAuthorizeAttribute(string requiredRole)
    {
        _requiredRole = requiredRole;
    }

    public async Task OnAuthorizationAsync(
        AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var session = httpContext.Session;

        var userId = session.GetInt32("UserId");
        var sessionRole = session.GetString("UserRole");

        // Check that a session exists.
        if (userId is null ||
            string.IsNullOrWhiteSpace(sessionRole))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Please log in to access this resource."
            });

            return;
        }

        // Verify the user and role against the database.
        var dbContext = httpContext.RequestServices
            .GetRequiredService<RaceDayContext>();

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.USER_ID == userId.Value)
            .Select(u => new
            {
                u.USER_ID,
                u.ROLE
            })
            .FirstOrDefaultAsync();

        // Reject sessions belonging to missing users.
        if (user == null)
        {
            session.Clear();

            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Your session is invalid. Please log in again."
            });

            return;
        }

        // Reject sessions with an outdated or incorrect role.
        if (!string.Equals(
                user.ROLE,
                sessionRole,
                StringComparison.OrdinalIgnoreCase))
        {
            session.Clear();

            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Your session is invalid. Please log in again."
            });

            return;
        }

        // Enforce the role required by the endpoint.
        if (_requiredRole != null &&
            !string.Equals(
                user.ROLE,
                _requiredRole,
                StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new
            {
                message =
                    "You do not have permission to access this resource."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

            return;
        }
    }
}

