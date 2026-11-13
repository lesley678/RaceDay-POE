
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class ProfileControllerTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly ProfileController _controller;

    public ProfileControllerTests()
    {
        _db = new TestDatabase();

        _db.Context.Users.Add(new User
        {
            USER_ID = 1,
            FIRSTNAME = "John",
            LASTNAME = "Smith",
            EMAIL = "john@example.com",
            PASSWORD_HASH = "test-hash",
            ROLE = "Participant"
        });

        _db.Context.SaveChanges();

        var httpContext = new DefaultHttpContext();
        var session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = session });

        session.SetInt32("UserId", 1);
        session.SetString("UserRole", "Participant");

        _controller = new ProfileController(_db.Context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Fact]
    public async Task GetProfile_ExistingUser_ReturnsOk()
    {
        var result = await _controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task UpdateProfile_ValidNames_UpdatesUser()
    {
        var request = new UpdateProfileRequest
        {
            FIRSTNAME = "  Jane ",
            LASTNAME = " Doe  "
        };

        var result = await _controller.UpdateProfile(request);

        Assert.IsType<OkObjectResult>(result);

        var user = await _db.Context.Users.FindAsync(1);

        Assert.NotNull(user);
        Assert.Equal("Jane", user.FIRSTNAME);
        Assert.Equal("Doe", user.LASTNAME);
    }

    [Fact]
    public async Task UpdateProfile_EmptyFirstName_ReturnsBadRequest()
    {
        var request = new UpdateProfileRequest
        {
            FIRSTNAME = " ",
            LASTNAME = "Smith"
        };

        var result = await _controller.UpdateProfile(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_NameOver50Characters_ReturnsBadRequest()
    {
        var request = new UpdateProfileRequest
        {
            FIRSTNAME = new string('A', 51),
            LASTNAME = "Smith"
        };

        var result = await _controller.UpdateProfile(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProfile_ValidNames_PreservesEmailAndRole()
    {
        var request = new UpdateProfileRequest
        {
            FIRSTNAME = "Jane",
            LASTNAME = "Doe"
        };

        await _controller.UpdateProfile(request);

        var user = await _db.Context.Users.FindAsync(1);

        Assert.NotNull(user);
        Assert.Equal("john@example.com", user.EMAIL);
        Assert.Equal("Participant", user.ROLE);
    }

    public void Dispose() => _db.Dispose();
}
