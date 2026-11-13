
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Attributes;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class SessionAuthorizeTests : IDisposable
{
    private readonly TestDatabase _db;

    public SessionAuthorizeTests()
    {
        _db = new TestDatabase();

        _db.Context.Users.Add(new User
        {
            USER_ID = 1,
            FIRSTNAME = "Test",
            LASTNAME = "User",
            EMAIL = "test@example.com",
            PASSWORD_HASH = "test-hash",
            ROLE = "Participant"
        });

        _db.Context.SaveChanges();
    }

    private AuthorizationFilterContext CreateContext(
        string? role,
        int? userId = 1)
    {
        var httpContext = new DefaultHttpContext();
        var session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = session });

        if (userId.HasValue)
            session.SetInt32("UserId", userId.Value);

        if (role != null)
            session.SetString("UserRole", role);

        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(_db.Context)
            .BuildServiceProvider();

        return new AuthorizationFilterContext(
            new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()),
            new List<IFilterMetadata>());
    }

    [Fact]
    public async Task MissingSession_ReturnsUnauthorized()
    {
        var context = CreateContext(null, null);
        var attribute = new SessionAuthorizeAttribute();

        await attribute.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(
            context.Result);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task CorrectSession_AllowsAccess()
    {
        var context = CreateContext("Participant");
        var attribute = new SessionAuthorizeAttribute();

        await attribute.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task WrongRole_ReturnsForbidden()
    {
        var context = CreateContext("Participant");
        var attribute = new SessionAuthorizeAttribute("Organiser");

        await attribute.OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeletedUser_ReturnsUnauthorized()
    {
        var context = CreateContext("Participant", 999);
        var attribute = new SessionAuthorizeAttribute();

        await attribute.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(
            context.Result);
        Assert.Equal(401, result.StatusCode);
    }

    public void Dispose() => _db.Dispose();
}

