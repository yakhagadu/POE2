using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Participant only - enrol in an event category.</summary>
    [HttpPost("enrolments")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Enrol(CreateEnrolmentDto dto)
    {
        var category = await _db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

        if (category is null)
            return NotFound(new { message = "Category not found." });

        // Get the participant profile for the current user
        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.UserId == CurrentUserId);

        if (participant is null)
            return BadRequest(new { message = "Participant profile not found." });

        // Check for duplicate enrolment
        var exists = await _db.Enrolments.AnyAsync(e =>
            e.ParticipantId == participant.ParticipantId &&
            e.CategoryId == dto.CategoryId);

        if (exists)
            return Conflict(new { message = "You are already enrolled in this category." });

        var enrolment = new Enrolment
        {
            ParticipantId = participant.ParticipantId,
            CategoryId = dto.CategoryId,
            PaymentReference = dto.PaymentReference,
            PaymentStatus = "Pending",
            EnrolmentStatus = "Active"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var response = new EnrolmentResponseDto(
            enrolment.EnrolmentId,
            participant.ParticipantId,
            $"{participant.User?.FirstName ?? "Participant"}",
            category.CategoryId,
            category.CategoryName,
            category.Event.EventName,
            enrolment.RegistrationDate,
            enrolment.PaymentStatus,
            enrolment.EnrolmentStatus,
            enrolment.RaceNumber);

        return CreatedAtAction(nameof(GetById), new { id = enrolment.EnrolmentId }, response);
    }

    /// <summary>Participant (self) or Organiser (event owner) - get enrolment details.</summary>
    [HttpGet("enrolments/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(EnrolmentResponseDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.Category).ThenInclude(c => c.Event).ThenInclude(ev => ev.Organiser)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        var isOwner = enrolment.Participant.UserId == CurrentUserId;
        var isEventOwner = enrolment.Category.Event.Organiser.UserId == CurrentUserId;

        if (!isOwner && !isEventOwner) return Forbidden();

        return Ok(new EnrolmentResponseDto(
            enrolment.EnrolmentId,
            enrolment.ParticipantId,
            $"{enrolment.Participant.User.FirstName} {enrolment.Participant.User.LastName}",
            enrolment.CategoryId,
            enrolment.Category.CategoryName,
            enrolment.Category.Event.EventName,
            enrolment.RegistrationDate,
            enrolment.PaymentStatus,
            enrolment.EnrolmentStatus,
            enrolment.RaceNumber));
    }

    /// <summary>Participant (self) - list own enrolments.</summary>
    [HttpGet("enrolments/participant/{participantId}")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetByParticipant(int participantId)
    {
        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.ParticipantId == participantId);

        if (participant is null) return NotFound(new { message = "Participant not found." });
        if (participant.UserId != CurrentUserId) return Forbidden();

        var enrolments = await _db.Enrolments
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.Category).ThenInclude(c => c.Event)
            .Where(e => e.ParticipantId == participantId)
            .Select(e => new EnrolmentResponseDto(
                e.EnrolmentId, e.ParticipantId,
                e.Participant.User.FirstName + " " + e.Participant.User.LastName,
                e.CategoryId, e.Category.CategoryName, e.Category.Event.EventName,
                e.RegistrationDate, e.PaymentStatus, e.EnrolmentStatus, e.RaceNumber))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Organiser (event owner) - list all enrolments for their event.</summary>
    [HttpGet("events/{eventId}/enrolments")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.Organiser.UserId != CurrentUserId) return Forbidden();

        var enrolments = await _db.Enrolments
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.Category)
            .Where(e => e.Category.EventId == eventId)
            .Select(e => new EnrolmentResponseDto(
                e.EnrolmentId, e.ParticipantId,
                e.Participant.User.FirstName + " " + e.Participant.User.LastName,
                e.CategoryId, e.Category.CategoryName, ev.EventName,
                e.RegistrationDate, e.PaymentStatus, e.EnrolmentStatus, e.RaceNumber))
            .ToListAsync();

        return Ok(enrolments);
    }

    /// <summary>Participant (self) - withdraw from an enrolment.</summary>
    [HttpDelete("enrolments/{id}")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.Enrolments
            .Include(e => e.Participant)
            .FirstOrDefaultAsync(e => e.EnrolmentId == id);

        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Participant.UserId != CurrentUserId) return Forbidden();

        enrolment.EnrolmentStatus = "Withdrawn";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private ObjectResult Forbidden() =>
        StatusCode(403, new { message = "You do not have access to this resource." });
}