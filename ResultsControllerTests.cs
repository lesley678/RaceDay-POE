
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class ResultsControllerTests : IDisposable
{
    private readonly TestDatabase _db;

    public ResultsControllerTests()
    {
        _db = new TestDatabase();

        _db.Context.Users.AddRange(
            new User
            {
                USER_ID = 1,
                FIRSTNAME = "John",
                LASTNAME = "Participant",
                EMAIL = "john@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Participant"
            },
            new User
            {
                USER_ID = 2,
                FIRSTNAME = "Jane",
                LASTNAME = "Participant",
                EMAIL = "jane@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Participant"
            },
            new User
            {
                USER_ID = 3,
                FIRSTNAME = "First",
                LASTNAME = "Organiser",
                EMAIL = "first@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Organiser"
            },
            new User
            {
                USER_ID = 4,
                FIRSTNAME = "Second",
                LASTNAME = "Organiser",
                EMAIL = "second@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Organiser"
            });

        _db.Context.Participants.AddRange(
            new Participant
            {
                PARTICIPANT_ID = 1,
                USER_ID = 1,
                DATE_OF_BIRTH = new DateTime(2000, 1, 1),
                GENDER = "Male",
                EMERGENCY_CONTACT = "0123456789"
            },
            new Participant
            {
                PARTICIPANT_ID = 2,
                USER_ID = 2,
                DATE_OF_BIRTH = new DateTime(2001, 1, 1),
                GENDER = "Female",
                EMERGENCY_CONTACT = "0123456789"
            });

        _db.Context.Organisers.AddRange(
            new Organiser
            {
                ORGANISER_ID = 1,
                USER_ID = 3,
                ORGANISATION_NAME = "First Club",
                CONTACT_NUMBER = "0123456789"
            },
            new Organiser
            {
                ORGANISER_ID = 2,
                USER_ID = 4,
                ORGANISATION_NAME = "Second Club",
                CONTACT_NUMBER = "0123456789"
            });

        _db.Context.Events.AddRange(
            new Event
            {
                EVENT_ID = 1,
                ORGANISER_ID = 1,
                EVENT_NAME = "First Race",
                DESCRIPTION = "First test event",
                EVENT_DATE = DateTime.UtcNow.AddDays(30),
                LOCATION = "Pretoria",
                STATUS = "Upcoming",
                EVENT_TYPE = "Run",
                DISTANCE = 10
            },
            new Event
            {
                EVENT_ID = 2,
                ORGANISER_ID = 2,
                EVENT_NAME = "Second Race",
                DESCRIPTION = "Second test event",
                EVENT_DATE = DateTime.UtcNow.AddDays(40),
                LOCATION = "Johannesburg",
                STATUS = "Upcoming",
                EVENT_TYPE = "Run",
                DISTANCE = 5
            });

        _db.Context.Categories.AddRange(
            new Category
            {
                CATEGORY_ID = 1,
                EVENT_ID = 1,
                CATEGORY_NAME = "10 KM",
                DISTANCE = 10,
                ENTRY_FEE = 50
            },
            new Category
            {
                CATEGORY_ID = 2,
                EVENT_ID = 2,
                CATEGORY_NAME = "5 KM",
                DISTANCE = 5,
                ENTRY_FEE = 30
            });

        _db.Context.Enrolments.AddRange(
            new Enrolment
            {
                ENROLMENT_ID = 1,
                PARTICIPANT_ID = 1,
                CATEGORY_ID = 1,
                ENROLMENT_DATE = DateTime.UtcNow,
                STATUS = "Active"
            },
            new Enrolment
            {
                ENROLMENT_ID = 2,
                PARTICIPANT_ID = 2,
                CATEGORY_ID = 2,
                ENROLMENT_DATE = DateTime.UtcNow,
                STATUS = "Active"
            });

        // Existing result for participant 1.
        _db.Context.Results.Add(new Result
        {
            RESULT_ID = 1,
            ENROLMENT_ID = 1,
            POSITION = 1,
            FINISH_TIME = TimeSpan.FromMinutes(45),
            RESULT_STATUS = "Finished"
        });

        _db.Context.SaveChanges();
    }

    private ResultsController CreateController(
        int userId,
        string role)
    {
        var httpContext = new DefaultHttpContext();
        var session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = session });

        session.SetInt32("UserId", userId);
        session.SetString("UserRole", role);

        return new ResultsController(_db.Context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Fact]
    public async Task CreateResult_ValidResult_ReturnsCreated()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 2,
            POSITION = 2,
            FINISH_TIME = TimeSpan.FromMinutes(50),
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<CreatedAtActionResult>(result);

        var saved = await _db.Context.Results
            .SingleAsync(r => r.ENROLMENT_ID == 2);

        Assert.Equal(2, saved.POSITION);
        Assert.Equal("Finished", saved.RESULT_STATUS);
    }

    [Fact]
    public async Task CreateResult_NonexistentEnrolment_ReturnsBadRequest()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 999,
            POSITION = 1,
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateResult_OtherOrganisersEvent_ReturnsForbidden()
    {
        // Organiser 1 attempts to create a result for event 2.
        var controller = CreateController(3, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 2,
            POSITION = 1,
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CreateResult_ZeroPosition_ReturnsBadRequest()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 2,
            POSITION = 0,
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateResult_NegativePosition_ReturnsBadRequest()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 2,
            POSITION = -1,
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateResult_DuplicateResult_ReturnsConflict()
    {
        var controller = CreateController(3, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 1,
            POSITION = 2,
            RESULT_STATUS = "Finished"
        });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(_db.Context.Results);
    }

    [Fact]
    public async Task CreateResult_EmptyStatus_ReturnsBadRequest()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.CreateResult(new Result
        {
            ENROLMENT_ID = 2,
            POSITION = 1,
            RESULT_STATUS = " "
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetResult_OwnResult_ReturnsOk()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetResult(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetResult_OtherParticipantsResult_ReturnsForbidden()
    {
        var controller = CreateController(2, "Participant");

        var result = await controller.GetResult(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetResult_NonexistentId_ReturnsNotFound()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetResult(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetResults_Participant_ReturnsOwnResults()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetResults();

        var ok = Assert.IsType<OkObjectResult>(result);
        var results = Assert.IsAssignableFrom<IEnumerable<Result>>(
            ok.Value);

        Assert.Single(results);
        Assert.All(results, r => Assert.Equal(1, r.ENROLMENT_ID));
    }

    [Fact]
    public async Task GetResults_Organiser_ReturnsOwnEventsResults()
    {
        var controller = CreateController(3, "Organiser");

        var result = await controller.GetResults();

        var ok = Assert.IsType<OkObjectResult>(result);
        var results = Assert.IsAssignableFrom<IEnumerable<Result>>(
            ok.Value);

        Assert.Single(results);
        Assert.All(results, r => Assert.Equal(1, r.ENROLMENT_ID));
    }

    [Fact]
    public async Task GetResult_OrganiserForAnotherEvent_ReturnsForbidden()
    {
        // Organiser 2 does not own the event associated with result 1.
        var controller = CreateController(4, "Organiser");

        var result = await controller.GetResult(1);

        Assert.IsType<ForbidResult>(result);
    }

    public void Dispose() => _db.Dispose();
}
