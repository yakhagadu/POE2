using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public UsersController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Update the current user's profile.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var user = await _db.Users
            .Include(u => u.Organiser)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;

        if (user.Role == "Organiser" && user.Organiser is not null)
        {
            if (!string.IsNullOrWhiteSpace(dto.OrganisationName)) user.Organiser.OrganisationName = dto.OrganisationName;
            if (!string.IsNullOrWhiteSpace(dto.ContactPhone)) user.Organiser.ContactPhone = dto.ContactPhone;
            if (!string.IsNullOrWhiteSpace(dto.Website)) user.Organiser.Website = dto.Website;
        }
        else if (user.Role == "Participant" && user.Participant is not null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Gender)) user.Participant.Gender = dto.Gender;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactName)) user.Participant.EmergencyContactName = dto.EmergencyContactName;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactPhone)) user.Participant.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (dto.MedicalConditions is not null) user.Participant.MedicalConditions = dto.MedicalConditions;
            if (dto.ClubName is not null) user.Participant.ClubName = dto.ClubName;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Profile updated." });
    }

    /// <summary>Change the current user's password.</summary>
    [HttpPut("profile/password")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password changed." });
    }
}