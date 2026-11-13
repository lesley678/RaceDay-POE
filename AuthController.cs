
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // POST: /api/auth/register

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var firstName = request.FIRSTNAME.Trim();
        var lastName = request.LASTNAME.Trim();
        var email = request.EMAIL.Trim();
        var roleInput = request.ROLE.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "First name, last name and email are required."
            });
        }

        string role;

        if (roleInput.Equals(
            "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            role = "Organiser";

            if (string.IsNullOrWhiteSpace(request.ORGANISATION_NAME) ||
                string.IsNullOrWhiteSpace(request.ORGANISER_CONTACT_NUMBER))
            {
                return BadRequest(new
                {
                    message = "Organisation name and organiser contact number are required."
                });
            }
        }
        else if (roleInput.Equals(
            "Participant", StringComparison.OrdinalIgnoreCase))
        {
            role = "Participant";

            if (!request.DATE_OF_BIRTH.HasValue ||
                request.DATE_OF_BIRTH.Value.Date >= DateTime.UtcNow.Date ||
                string.IsNullOrWhiteSpace(request.GENDER) ||
                string.IsNullOrWhiteSpace(request.EMERGENCY_CONTACT))
            {
                return BadRequest(new
                {
                    message = "Provide a valid date of birth, gender and emergency contact."
                });
            }
        }
        else
        {
            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        if (await _context.Users.AnyAsync(
            u => u.EMAIL.ToLower() == email.ToLower()))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FIRSTNAME = firstName,
            LASTNAME = lastName,
            EMAIL = email,
            ROLE = role,
            CREATED_AT = DateTime.UtcNow
        };

        user.PASSWORD_HASH =
            _passwordHasher.HashPassword(user, request.PASSWORD);

        // Keep the account and its role-specific profile consistent.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (role == "Organiser")
            {
                _context.Organisers.Add(new Organiser
                {
                    USER_ID = user.USER_ID,
                    ORGANISATION_NAME =
                        request.ORGANISATION_NAME!.Trim(),
                    CONTACT_NUMBER =
                        request.ORGANISER_CONTACT_NUMBER!.Trim()
                });
            }
            else
            {
                _context.Participants.Add(new Participant
                {
                    USER_ID = user.USER_ID,
                    DATE_OF_BIRTH = request.DATE_OF_BIRTH!.Value,
                    GENDER = request.GENDER!.Trim(),
                    EMERGENCY_CONTACT =
                        request.EMERGENCY_CONTACT!.Trim()
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "Registration could not be completed. Check whether the email or profile already exists."
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Registration successful.",
            user = new
            {
                user.USER_ID,
                user.FIRSTNAME,
                user.LASTNAME,
                user.EMAIL,
                user.ROLE,
                user.CREATED_AT
            }
        });
    }


    // POST: /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var email = request.EMAIL.Trim();

        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.EMAIL.ToLower() == email.ToLower());

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            user,
            user.PASSWORD_HASH,
            request.PASSWORD);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        // Replace any existing authenticated session.
        HttpContext.Session.Clear();

        HttpContext.Session.SetInt32("UserId", user.USER_ID);
        HttpContext.Session.SetString("UserRole", user.ROLE);

        return Ok(new
        {
            message = "Login successful.",
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

    // POST: /api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return Ok(new
        {
            message = "Logout successful."
        });
    }
}


public class RegisterRequest
{
    [Required, MaxLength(50)]
    public string FIRSTNAME { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LASTNAME { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string EMAIL { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string PASSWORD { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ROLE { get; set; } = "Participant";

    // Required when ROLE is Organiser
    [MaxLength(100)]
    public string? ORGANISATION_NAME { get; set; }

    [MaxLength(20)]
    public string? ORGANISER_CONTACT_NUMBER { get; set; }

    // Required when ROLE is Participant
    public DateTime? DATE_OF_BIRTH { get; set; }

    [MaxLength(20)]
    public string? GENDER { get; set; }

    [MaxLength(100)]
    public string? EMERGENCY_CONTACT { get; set; }
}


public class LoginRequest
{
    [Required, EmailAddress, MaxLength(100)]
    public string EMAIL { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string PASSWORD { get; set; } = string.Empty;
}

