
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class EnrolmentsControllerTests : IDisposable
{
    private readonly TestDatabase _db;

    public EnrolmentsControllerTests()
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
                LASTNAME = "Other",
                EMAIL = "jane@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Participant"
            },
            new User
            {
                USER_ID = 3,
                FIRSTNAME = "Event",
                LASTNAME = "Organiser",
                EMAIL = "organiser@example.com",
                PASSWORD_HASH = "test-hash",
                ROLE = "Organiser"
            },
            new User
            {
                USER_ID = 4,
                FIRSTNAME = "Another",
                LASTNAME = "Organiser",
                EMAIL = "other@example.com",
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
                DATE_OF_BIRTH = new DateTime(2000, 1, 1),
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

        _db.Context.Enrolments.Add(
            new Enrolment
            {
                ENROLMENT_ID = 1,
                PARTICIPANT_ID = 1,
                CATEGORY_ID = 1,
                ENROLMENT_DATE = DateTime.UtcNow,
                STATUS = "Active"
            });

        _db.Context.SaveChanges();
    }

    private EnrolmentsController CreateController(
        int userId,
        string role)
    {
        var httpContext = new DefaultHttpContext();
        var session = new TestSession();

        httpContext.Features.Set<ISessionFeature>(
            new TestSessionFeature { Session = session });

        session.SetInt32("UserId", userId);
        session.SetString("UserRole", role);

        return new EnrolmentsController(_db.Context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Fact]
    public async Task CreateEnrolment_ValidCategory_CreatesEnrolment()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.CreateEnrolment(
            new Enrolment
            {
                PARTICIPANT_ID = 999,
                CATEGORY_ID = 2
            });

        Assert.IsType<CreatedAtActionResult>(result);

        var saved = await _db.Context.Enrolments
            .SingleAsync(e => e.CATEGORY_ID == 2);

        Assert.Equal(1, saved.PARTICIPANT_ID);
        Assert.Equal("Pending", saved.STATUS);
    }

    [Fact]
    public async Task CreateEnrolment_NonexistentCategory_ReturnsBadRequest()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.CreateEnrolment(
            new Enrolment { CATEGORY_ID = 999 });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Single(_db.Context.Enrolments);
    }

    [Fact]
    public async Task CreateEnrolment_DuplicateCategory_ReturnsConflict()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.CreateEnrolment(
            new Enrolment { CATEGORY_ID = 1 });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(_db.Context.Enrolments);
    }

    [Fact]
    public async Task GetEnrolment_OwnEnrolment_ReturnsOk()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetEnrolment(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetEnrolment_OtherParticipantsEnrolment_ReturnsForbidden()
    {
        var controller = CreateController(2, "Participant");

        var result = await controller.GetEnrolment(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetEnrolment_NonexistentId_ReturnsNotFound()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetEnrolment(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetEnrolments_Participant_ReturnsOwnEnrolments()
    {
        var controller = CreateController(1, "Participant");

        var result = await controller.GetEnrolments();

        var ok = Assert.IsType<OkObjectResult>(result);
        var enrolments = Assert.IsAssignableFrom<
            IEnumerable<Enrolment>>(ok.Value);

        Assert.Single(enrolments);
        Assert.All(enrolments,
            e => Assert.Equal(1, e.PARTICIPANT_ID));
    }

    [Fact]
    public async Task GetEnrolments_Organiser_ReturnsOwnEventsEnrolments()
    {
        var controller = CreateController(3, "Organiser");

        var result = await controller.GetEnrolments();

        var ok = Assert.IsType<OkObjectResult>(result);
        var enrolments = Assert.IsAssignableFrom<
            IEnumerable<Enrolment>>(ok.Value);

        Assert.Single(enrolments);
        Assert.All(enrolments,
            e => Assert.Equal(1, e.CATEGORY_ID));
    }

    [Fact]
    public async Task GetEnrolment_OtherOrganisersEvent_ReturnsForbidden()
    {
        var controller = CreateController(4, "Organiser");

        var result = await controller.GetEnrolment(1);

        Assert.IsType<ForbidResult>(result);
    }

    public void Dispose() => _db.Dispose();
}
        