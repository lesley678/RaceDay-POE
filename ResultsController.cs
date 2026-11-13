
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/results
    // Participants see their own results.
    // Organisers see results for their own events.
    [SessionAuthorize]
    [HttpGet]
    public async Task<IActionResult> GetResults()
    {
        var userId = SessionHelper.GetUserId(HttpContext);
        var role = SessionHelper.GetUserRole(HttpContext);

        if (userId == null || string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized(new { message = "Please log in first." });
        }

        if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.USER_ID == userId.Value);

            if (participant == null)
            {
                return Forbid();
            }

            var results = await _context.Results
                .AsNoTracking()
                .Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.PARTICIPANT_ID == participant.PARTICIPANT_ID)
                .ToListAsync();

            return Ok(results);
        }

        if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var organiser = await _context.Organisers
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

            if (organiser == null)
            {
                return Forbid();
            }

            var results = await _context.Results
                .AsNoTracking()
                .Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.Category != null &&
                    r.Enrolment.Category.Event != null &&
                    r.Enrolment.Category.Event.ORGANISER_ID ==
                        organiser.ORGANISER_ID)
                .ToListAsync();

            return Ok(results);
        }

        return Forbid();
    }

    // GET: api/results/1
    // Only the participant who owns the result or the event's organiser can view it.
    [SessionAuthorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetResult(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext);
        var role = SessionHelper.GetUserRole(HttpContext);

        if (userId == null || string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized(new { message = "Please log in first." });
        }

        var result = await _context.Results
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RESULT_ID == id);

        if (result == null)
        {
            return NotFound(new { message = "Result not found." });
        }

        if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            var participant = await _context.Participants
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.USER_ID == userId.Value);

            if (participant == null)
            {
                return Forbid();
            }

            var ownsResult = await _context.Results
                .AnyAsync(r =>
                    r.RESULT_ID == id &&
                    r.Enrolment != null &&
                    r.Enrolment.PARTICIPANT_ID ==
                        participant.PARTICIPANT_ID);

            if (!ownsResult)
            {
                return Forbid();
            }

            return Ok(result);
        }

        if (role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
        {
            var organiser = await _context.Organisers
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

            var ownsEvent = organiser != null &&
                await _context.Results.AnyAsync(r =>
                    r.RESULT_ID == id &&
                    r.Enrolment != null &&
                    r.Enrolment.Category != null &&
                    r.Enrolment.Category.Event != null &&
                    r.Enrolment.Category.Event.ORGANISER_ID ==
                        organiser.ORGANISER_ID);

            if (!ownsEvent)
            {
                return Forbid();
            }

            return Ok(result);
        }

        return Forbid();
    }

    // POST: api/results
    // Only an organiser can create results for their own events.
    [SessionAuthorize("Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateResult(
        [FromBody] Result result)
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId == null)
        {
            return Unauthorized(new { message = "Please log in first." });
        }

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

        if (organiser == null)
        {
            return Forbid();
        }

        if (result.POSITION <= 0)
        {
            return BadRequest(new
            {
                message = "Finishing position must be greater than zero."
            });
        }

        if (string.IsNullOrWhiteSpace(result.RESULT_STATUS))
        {
            return BadRequest(new
            {
                message = "Result status is required."
            });
        }

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
            .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e =>
                e.ENROLMENT_ID == result.ENROLMENT_ID);

        if (enrolment == null)
        {
            return BadRequest(new { message = "Enrolment does not exist." });
        }

        if (enrolment.Category?.Event == null ||
            enrolment.Category.Event.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        var alreadyHasResult = await _context.Results
            .AnyAsync(r => r.ENROLMENT_ID == result.ENROLMENT_ID);

        if (alreadyHasResult)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        result.RESULT_STATUS = result.RESULT_STATUS.Trim();

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.RESULT_ID },
            result);
    }
}

