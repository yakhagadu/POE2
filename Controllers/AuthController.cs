using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(RaceDayDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    /// <summary>Registers a new user as either an Organiser or a Participant.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (!new[] { "Organiser", "Participant" }.Contains(dto.Role))
            return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role
        };

        if (dto.Role == "Organiser")
        {
            user.Organiser = new EventOrganiser
            {
                OrganisationName = dto.OrganisationName ?? $"{dto.FirstName} {dto.LastName}",
                ContactPhone = dto.ContactPhone ?? string.Empty,
                Website = dto.Website
            };
        }
        else
        {
            user.Participant = new Participant
            {
                DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow.AddYears(-20),
                Gender = dto.Gender,
                EmergencyContactName = dto.EmergencyContactName ?? "N/A",
                EmergencyContactPhone = dto.EmergencyContactPhone ?? "N/A",
                MedicalConditions = dto.MedicalConditions,
                ClubName = dto.ClubName
            };
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expires) = _tokenService.CreateToken(user);

        return StatusCode(201, new AuthResponseDto(
            token, user.UserId, user.Email, user.FirstName, user.LastName, user.Role, expires));
    }

    /// <summary>Logs in an existing user and returns a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        if (!user.IsActive)
            return Unauthorized(new { message = "Account is inactive." });

        var (token, expires) = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto(
            token, user.UserId, user.Email, user.FirstName, user.LastName, user.Role, expires));
    }

    /// <summary>Returns the currently authenticated user's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users
            .Include(u => u.Organiser)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null) return NotFound();

        return Ok(new
        {
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            Organiser = user.Organiser,
            Participant = user.Participant
        });
    }
}