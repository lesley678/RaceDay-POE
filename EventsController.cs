

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/events
    // Public: anyone can view available events.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/1
    // Public: view a specific event.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var raceEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EVENT_ID == id);

        if (raceEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        return Ok(raceEvent);
    }

    // POST: api/events
    // Only logged-in organisers can create events.
    [SessionAuthorize("Organiser")]
    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent(
        [FromBody] Event raceEvent)
    {
        if (raceEvent.DISTANCE <= 0)
        {
            return BadRequest(new
            {
                message = "Distance must be greater than zero."
            });
        }

        var validTypes = new[] { "Run", "Walk", "Cycle" };

        if (string.IsNullOrWhiteSpace(raceEvent.EVENT_TYPE) ||
            !validTypes.Contains(
                raceEvent.EVENT_TYPE,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Event type must be Run, Walk, or Cycle."
            });
        }

        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "Please log in first."
            });
        }

        // Get the organiser linked to the logged-in user.
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

        if (organiser == null)
        {
            return Forbid();
        }

        // Never trust the organiser ID supplied in the request body.
        raceEvent.ORGANISER_ID = organiser.ORGANISER_ID;

        raceEvent.EVENT_NAME = raceEvent.EVENT_NAME?.Trim() ?? "";
        raceEvent.DESCRIPTION = raceEvent.DESCRIPTION?.Trim() ?? "";
        raceEvent.LOCATION = raceEvent.LOCATION?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(raceEvent.EVENT_NAME) ||
            string.IsNullOrWhiteSpace(raceEvent.DESCRIPTION) ||
            string.IsNullOrWhiteSpace(raceEvent.LOCATION))
        {
            return BadRequest(new
            {
                message = "Event name, description and location are required."
            });
        }

        raceEvent.EVENT_TYPE = validTypes.First(
            t => t.Equals(
                raceEvent.EVENT_TYPE,
                StringComparison.OrdinalIgnoreCase));

        raceEvent.STATUS = "Upcoming";

        _context.Events.Add(raceEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = raceEvent.EVENT_ID },
            raceEvent);
    }

    // PUT: api/events/1
    // Only the organiser who owns the event can update it.
    [SessionAuthorize("Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] Event updatedEvent)
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "Please log in first."
            });
        }

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

        if (organiser == null)
        {
            return Forbid();
        }

        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EVENT_ID == id);

        if (existingEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (existingEvent.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        if (updatedEvent.DISTANCE <= 0)
        {
            return BadRequest(new
            {
                message = "Distance must be greater than zero."
            });
        }

        var validTypes = new[] { "Run", "Walk", "Cycle" };

        if (string.IsNullOrWhiteSpace(updatedEvent.EVENT_TYPE) ||
            !validTypes.Contains(
                updatedEvent.EVENT_TYPE,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Event type must be Run, Walk, or Cycle."
            });
        }

        var eventName = updatedEvent.EVENT_NAME?.Trim() ?? "";
        var description = updatedEvent.DESCRIPTION?.Trim() ?? "";
        var location = updatedEvent.LOCATION?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(eventName) ||
            string.IsNullOrWhiteSpace(description) ||
            string.IsNullOrWhiteSpace(location))
        {
            return BadRequest(new
            {
                message = "Event name, description and location are required."
            });
        }

        existingEvent.EVENT_NAME = eventName;
        existingEvent.DESCRIPTION = description;
        existingEvent.EVENT_DATE = updatedEvent.EVENT_DATE;
        existingEvent.LOCATION = location;
        existingEvent.EVENT_TYPE = validTypes.First(
            t => t.Equals(
                updatedEvent.EVENT_TYPE,
                StringComparison.OrdinalIgnoreCase));
        existingEvent.DISTANCE = updatedEvent.DISTANCE;

        // Do not allow the request to transfer ownership to another organiser.
        // Keep the existing event status unless you add a dedicated status rule.
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/events/1
    // Only the organiser who owns the event can delete it.
    [SessionAuthorize("Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "Please log in first."
            });
        }

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.USER_ID == userId.Value);

        if (organiser == null)
        {
            return Forbid();
        }

        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EVENT_ID == id);

        if (existingEvent == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (existingEvent.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        _context.Events.Remove(existingEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

