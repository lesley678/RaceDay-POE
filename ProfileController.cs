
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Attributes;
using RaceDay.API.Data;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/profile")]
[SessionAuthorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ProfileController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "Please log in first."
            });
        }

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.USER_ID == userId.Value)
            .Select(u => new
            {
                u.USER_ID,
                u.FIRSTNAME,
                u.LASTNAME,
                u.EMAIL,
                u.ROLE,
                u.CREATED_AT
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            HttpContext.Session.Clear();

            return Unauthorized(new
            {
                message =
                    "Your session is invalid. Please log in again."
            });
        }

        return Ok(user);
    }

    // PUT: api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request)
    {
        var userId = SessionHelper.GetUserId(HttpContext);

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "Please log in first."
            });
        }

        var firstName = request.FIRSTNAME?.Trim();
        var lastName = request.LASTNAME?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new
            {
                message =
                    "First name and last name are required."
            });
        }

        if (firstName.Length > 50 || lastName.Length > 50)
        {
            return BadRequest(new
            {
                message = "Names cannot exceed 50 characters."
            });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.USER_ID == userId.Value);

        if (user == null)
        {
            HttpContext.Session.Clear();

            return Unauthorized(new
            {
                message =
                    "Your session is invalid. Please log in again."
            });
        }

        user.FIRSTNAME = firstName;
        user.LASTNAME = lastName;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Profile updated successfully.",
            user = new
            {
                user.USER_ID,
                user.FIRSTNAME,
                user.LASTNAME,
                user.EMAIL,
                user.ROLE
            }
        });
    }
}

public class UpdateProfileRequest
{
    public string FIRSTNAME { get; set; } = string.Empty;

    public string LASTNAME { get; set; } = string.Empty;
}

