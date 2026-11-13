
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.API.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task CreateEvent_WithZeroDistance_ReturnsBadRequest()
    {
        // Arrange: create a separate in-memory database for this test.
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new RaceDayContext(options);

        var controller = new EventsController(context);

        // Supply an HTTP context for the controller.
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var raceEvent = new Event
        {
            DISTANCE = 0,
            EVENT_TYPE = "Run",
            EVENT_NAME = "Test Race",
            DESCRIPTION = "A test event",
            LOCATION = "Pretoria"
        };

        // Act: call the controller directly.
        var result = await controller.CreateEvent(raceEvent);

        // Assert: zero distance must be rejected.
        Assert.IsType<BadRequestObjectResult>(result.Result);

        // Ensure that no event was saved.
        Assert.Empty(await context.Events.ToListAsync());
    }
}
