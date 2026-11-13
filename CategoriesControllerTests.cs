
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Models;
using RaceDay.API.Tests;

namespace RaceDay.API.Tests;

public class CategoriesControllerTests : IDisposable
{
    private readonly TestDatabase _db;
    private readonly CategoriesController _controller;
    private readonly TestSession _session;

    private const int UserId = 1;
    private const int OrganiserId = 1;
    private const int EventId = 1;

    public CategoriesControllerTests()
    {
        _db = new TestDatabase();

        _db.Context.Users.Add(new User
        {
            USER_ID = UserId,
            FIRSTNAME = "Test",
            LASTNAME = "Organiser",
            EMAIL = "organiser@example.com",
            PASSWORD_HASH = "test-hash",
            ROLE = "Organiser"
        });

        _db.Context.Organisers.Add(new Organiser
        {
            ORGANISER_ID = OrganiserId,
            USER_ID = UserId,
            ORGANISATION_NAME = "Race Club",
            CONTACT_NUMBER = "0123456789"
        });

        _db.Context.Events.Add(new Event
        {
            EVENT_ID = EventId,
            ORGANISER_ID = OrganiserId,
            EVENT_NAME = "City Run",
            DESCRIPTION = "Test event",
            EVENT_DATE = DateTime.UtcNow.AddDays(30),
            LOCATION = "Johannesburg",
            STATUS = "Upcoming",
            EVENT_TYPE = "Run",
            DISTANCE = 10
        });

        _db.Context.SaveChanges();

        var httpContext = new DefaultHttpContext();
        _session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = _session });

        _session.SetInt32("UserId", UserId);
        _session.SetString("UserRole", "Organiser");

        _controller = new CategoriesController(_db.Context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        var result = await _controller.GetCategories();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCategory_InvalidId_ReturnsNotFound()
    {
        var result = await _controller.GetCategory(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateCategory_EmptyName_ReturnsBadRequest()
    {
        var category = new Category
        {
            EVENT_ID = EventId,
            CATEGORY_NAME = " ",
            DISTANCE = 5,
            ENTRY_FEE = 10
        };

        var result = await _controller.CreateCategory(category);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Empty(await _db.Context.Categories.ToListAsync());
    }

    [Fact]
    public async Task CreateCategory_ValidData_CreatesCategory()
    {
        var category = new Category
        {
            EVENT_ID = EventId,
            CATEGORY_NAME = "  10 KM  ",
            DISTANCE = 10,
            ENTRY_FEE = 50
        };

        var result = await _controller.CreateCategory(category);

        var created = Assert.IsType<CreatedAtActionResult>(
            result.Result);

        Assert.Equal("GetCategory", created.ActionName);

        var saved = await _db.Context.Categories.SingleAsync();

        Assert.Equal("10 KM", saved.CATEGORY_NAME);
        Assert.Equal(10, saved.DISTANCE);
        Assert.Equal(50, saved.ENTRY_FEE);
    }

    [Fact]
    public async Task CreateCategory_InvalidDistance_ReturnsBadRequest()
    {
        var category = new Category
        {
            EVENT_ID = EventId,
            CATEGORY_NAME = "5 KM",
            DISTANCE = 0,
            ENTRY_FEE = 10
        };

        var result = await _controller.CreateCategory(category);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateCategory_NegativeEntryFee_ReturnsBadRequest()
    {
        var category = new Category
        {
            EVENT_ID = EventId,
            CATEGORY_NAME = "5 KM",
            DISTANCE = 5,
            ENTRY_FEE = -1
        };

        var result = await _controller.CreateCategory(category);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateCategory_NonexistentEvent_ReturnsBadRequest()
    {
        var category = new Category
        {
            EVENT_ID = 999,
            CATEGORY_NAME = "5 KM",
            DISTANCE = 5,
            ENTRY_FEE = 10
        };

        var result = await _controller.CreateCategory(category);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    public void Dispose() => _db.Dispose();
}

internal sealed class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _values = new();

    public bool IsAvailable => true;
    public string Id => "test-session";
    public IEnumerable<string> Keys => _values.Keys;

    public void Clear() => _values.Clear();

    public Task CommitAsync(
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task LoadAsync(
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public void Remove(string key) => _values.Remove(key);

    public void Set(string key, byte[] value) =>
        _values[key] = value;

    public bool TryGetValue(string key, out byte[] value) =>
        _values.TryGetValue(key, out value!);
}

internal sealed class TestSessionFeature : ISessionFeature
{
    public ISession Session { get; set; } = null!;
}
