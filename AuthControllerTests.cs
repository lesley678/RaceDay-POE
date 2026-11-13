
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RaceDay.API.Controllers;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class AuthControllerTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly PasswordHasher<User> _hasher;
    private readonly AuthController _controller;
    private readonly TestSession _session;

    public AuthControllerTests()
    {
        _db = new TestDatabase();
        _hasher = new PasswordHasher<User>();

        var httpContext = new DefaultHttpContext();
        _session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = _session });

        _controller = new AuthController(_db.Context, _hasher)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private RegisterRequest ValidParticipantRequest() =>
        new()
        {
            FIRSTNAME = "John",
            LASTNAME = "Smith",
            EMAIL = "john@example.com",
            PASSWORD = "Password123!",
            ROLE = "Participant",
            DATE_OF_BIRTH = DateTime.UtcNow.AddYears(-20),
            GENDER = "Male",
            EMERGENCY_CONTACT = "0123456789"
        };

    [Fact]
    public async Task Register_ValidParticipant_ReturnsCreated()
    {
        var request = ValidParticipantRequest();

        var result = await _controller.Register(request);

        Assert.IsType<ObjectResult>(result);
        var response = (ObjectResult)result;
        Assert.Equal(201, response.StatusCode);

        Assert.Equal(1, _db.Context.Users.Count());
        Assert.Equal(1, _db.Context.Participants.Count());

        var user = _db.Context.Users.Single();
        Assert.Equal("Participant", user.ROLE);
        Assert.NotEqual("Password123!", user.PASSWORD_HASH);
    }

    [Fact]
    public async Task Register_ValidOrganiser_ReturnsCreated()
    {
        var request = new RegisterRequest
        {
            FIRSTNAME = "Jane",
            LASTNAME = "Jones",
            EMAIL = "jane@example.com",
            PASSWORD = "Password123!",
            ROLE = "Organiser",
            ORGANISATION_NAME = "City Running Club",
            ORGANISER_CONTACT_NUMBER = "0123456789"
        };

        var result = await _controller.Register(request);

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, response.StatusCode);

        Assert.Equal(1, _db.Context.Users.Count());
        Assert.Equal(1, _db.Context.Organisers.Count());
    }

    [Fact]
    public async Task Register_ExistingEmail_ReturnsConflict()
    {
        var request = ValidParticipantRequest();

        await _controller.Register(request);

        var duplicate = ValidParticipantRequest();
        duplicate.EMAIL = "JOHN@example.com";

        var result = await _controller.Register(duplicate);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(_db.Context.Users);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var request = ValidParticipantRequest();
        request.ROLE = "Administrator";

        var result = await _controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(_db.Context.Users);
    }

    [Fact]
    public async Task Register_MissingDateOfBirth_ReturnsBadRequest()
    {
        var request = ValidParticipantRequest();
        request.DATE_OF_BIRTH = null;

        var result = await _controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(_db.Context.Users);
    }

    [Fact]
    public async Task Register_OrganiserMissingDetails_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            FIRSTNAME = "Jane",
            LASTNAME = "Jones",
            EMAIL = "jane@example.com",
            PASSWORD = "Password123!",
            ROLE = "Organiser"
        };

        var result = await _controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(_db.Context.Users);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkAndCreatesSession()
    {
        var user = new User
        {
            FIRSTNAME = "John",
            LASTNAME = "Smith",
            EMAIL = "john@example.com",
            ROLE = "Participant"
        };

        user.PASSWORD_HASH =
            _hasher.HashPassword(user, "Password123!");

        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();

        var result = await _controller.Login(new LoginRequest
        {
            EMAIL = "JOHN@example.com",
            PASSWORD = "Password123!"
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user.USER_ID, _session.GetInt32("UserId"));
        Assert.Equal("Participant", _session.GetString("UserRole"));
    }

    [Fact]
    public async Task Login_IncorrectPassword_ReturnsUnauthorized()
    {
        var user = new User
        {
            FIRSTNAME = "John",
            LASTNAME = "Smith",
            EMAIL = "john@example.com",
            ROLE = "Participant"
        };

        user.PASSWORD_HASH =
            _hasher.HashPassword(user, "CorrectPassword123!");

        _db.Context.Users.Add(user);
        await _db.Context.SaveChangesAsync();

        var result = await _controller.Login(new LoginRequest
        {
            EMAIL = "john@example.com",
            PASSWORD = "WrongPassword123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Null(_session.GetInt32("UserId"));
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var result = await _controller.Login(new LoginRequest
        {
            EMAIL = "unknown@example.com",
            PASSWORD = "Password123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void Logout_ClearsSessionAndReturnsOk()
    {
        _session.SetInt32("UserId", 1);
        _session.SetString("UserRole", "Participant");

        var result = _controller.Logout();

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(_session.GetInt32("UserId"));
        Assert.Null(_session.GetString("UserRole"));
    }

    public void Dispose() => _db.Dispose();
}
