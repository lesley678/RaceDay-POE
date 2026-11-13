
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/enrolments
    [SessionAuthorize]
    [HttpGet]
    public async Task<IActionResult> GetEnrolments()
    {
        var userId = SessionHelper.GetUserId(HttpContext);
        var role = SessionHelper.GetUserRole(HttpContext);

        if (userId == null || string.IsNullOrWhiteSpace(role))
            return Unauthorized(new { message = "Please log in first." });

        if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.USER_ID == userId.Value);

            if (participant == null)
                return Forbid();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.PARTICIPANT_ID == participant.PARTICIPANT_ID)
                .ToListAsync();

            return Ok(enrolments);
        }

        if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var organiser = await _context.Organisers
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

            if (organiser == null)
                return Forbid();

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.Category != null &&
                            e.Category.Event != null &&
                            e.Category.Event.ORGANISER_ID == organiser.ORGANISER_ID)
                .ToListAsync();

            return Ok(enrolments);
        }

        return Forbid();
    }

    // GET: api/enrolments/1
    [SessionAuthorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEnrolment(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext);
        var role = SessionHelper.GetUserRole(HttpContext);

        if (userId == null || string.IsNullOrWhiteSpace(role))
            return Unauthorized(new { message = "Please log in first." });

        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ENROLMENT_ID == id);

        if (enrolment == null)
            return NotFound(new { message = "Enrolment not found." });

        if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.USER_ID == userId.Value);

            if (participant == null ||
                enrolment.PARTICIPANT_ID != participant.PARTICIPANT_ID)
                return Forbid();

            return Ok(enrolment);
        }

        if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var organiser = await _context.Organisers
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

            var ownsEvent = organiser != null &&
                await _context.Enrolments.AnyAsync(e =>
                    e.ENROLMENT_ID == id &&
                    e.Category != null &&
                    e.Category.Event != null &&
                    e.Category.Event.ORGANISER_ID == organiser.ORGANISER_ID);

            if (!ownsEvent)
                return Forbid();

            return Ok(enrolment);
        }

        return Forbid();
    }

    // POST: api/enrolments
    [SessionAuthorize("Participant")]
    [HttpPost]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] Enrolment enrolment)
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId == null)
            return Unauthorized(new { message = "Please log in first." });

        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.USER_ID == userId.Value);

        if (participant == null)
            return Forbid();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CATEGORY_ID == enrolment.CATEGORY_ID);

        if (category == null)
            return BadRequest(new { message = "Category does not exist." });

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e =>
                e.PARTICIPANT_ID == participant.PARTICIPANT_ID &&
                e.CATEGORY_ID == enrolment.CATEGORY_ID);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this category."
            });
        }

        // Assign trusted values from the authenticated session.
        enrolment.PARTICIPANT_ID = participant.PARTICIPANT_ID;
        enrolment.ENROLMENT_DATE = DateTime.UtcNow;
        enrolment.STATUS = "Pending";

        // Do not accept payment amounts from the participant's request.
        // Payment should be handled separately.

        try
        {
            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.ENROLMENT_ID },
                enrolment
            );
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("ENROLMENT DATABASE ERROR:");
            Console.WriteLine(ex.InnerException?.Message ?? ex.Message);

            return StatusCode(500, new
            {
                message = "Unable to save enrolment. Please check the enrolment database configuration."
            });
        }
    }
}
