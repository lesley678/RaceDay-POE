
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/categories
    // Public: view all categories.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/1
    // Public: view a specific category.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CATEGORY_ID == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        return Ok(category);
    }

    // POST: api/categories
    // Only the organiser who owns the event can create its categories.
    [SessionAuthorize("Organiser")]
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(
        [FromBody] Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CATEGORY_NAME))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (category.DISTANCE <= 0)
        {
            return BadRequest(new
            {
                message = "Category distance must be greater than zero."
            });
        }

        if (category.ENTRY_FEE < 0)
        {
            return BadRequest(new
            {
                message = "Entry fee cannot be negative."
            });
        }

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

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EVENT_ID == category.EVENT_ID);

        if (raceEvent == null)
        {
            return BadRequest(new { message = "Event does not exist." });
        }

        if (raceEvent.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        category.CATEGORY_NAME = category.CATEGORY_NAME.Trim();

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CATEGORY_ID },
            category);
    }

    // PUT: api/categories/1
    // Only the organiser who owns the event can update its categories.
    [SessionAuthorize("Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] Category updatedCategory)
    {
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.CATEGORY_ID == id);

        if (existingCategory == null)
        {
            return NotFound(new { message = "Category not found." });
        }

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

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EVENT_ID == existingCategory.EVENT_ID);

        if (raceEvent == null ||
            raceEvent.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        // Do not allow a category to be moved to a different event
        // through the request body.
        if (updatedCategory.EVENT_ID != existingCategory.EVENT_ID)
        {
            return BadRequest(new
            {
                message = "A category cannot be moved to another event."
            });
        }

        if (string.IsNullOrWhiteSpace(updatedCategory.CATEGORY_NAME))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (updatedCategory.DISTANCE <= 0)
        {
            return BadRequest(new
            {
                message = "Category distance must be greater than zero."
            });
        }

        if (updatedCategory.ENTRY_FEE < 0)
        {
            return BadRequest(new
            {
                message = "Entry fee cannot be negative."
            });
        }

        existingCategory.CATEGORY_NAME =
            updatedCategory.CATEGORY_NAME.Trim();

        existingCategory.DISTANCE = updatedCategory.DISTANCE;
        existingCategory.ENTRY_FEE = updatedCategory.ENTRY_FEE;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/categories/1
    // Only the organiser who owns the event can delete its category.
    [SessionAuthorize("Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CATEGORY_ID == id);

        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

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

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EVENT_ID == category.EVENT_ID);

        if (raceEvent == null ||
            raceEvent.ORGANISER_ID != organiser.ORGANISER_ID)
        {
            return Forbid();
        }

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.CATEGORY_ID == id);

        if (hasEnrolments)
        {
            return Conflict(new
            {
                message = "This category cannot be deleted because it has enrolments."
            });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

