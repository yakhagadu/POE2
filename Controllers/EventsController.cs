using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

    /// <summary>Public - list all events (with optional filtering).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), 200)]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? type)
    {
        var query = _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .Where(e => !e.IsArchived)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => e.Status == status);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(e => e.EventType == type);

        var events = await query.Select(e => new EventResponseDto(
            e.EventId, e.EventName, e.EventType, e.Description, e.DateTime,
            e.Location, e.MaxParticipants, e.EntryFee, e.RouteDescription, e.Status,
            e.OrganiserId, e.Organiser.OrganisationName, e.Categories.Count, e.CreatedAt))
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>Public - get event by id (with categories).</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev is null) return NotFound(new { message = "Event not found." });

        return Ok(new
        {
            ev.EventId,
            ev.EventName,
            ev.EventType,
            ev.Description,
            ev.DateTime,
            ev.Location,
            ev.MaxParticipants,
            ev.EntryFee,
            ev.RouteDescription,
            ev.Status,
            Organiser = new { ev.OrganiserId, ev.Organiser.OrganisationName },
            Categories = ev.Categories.Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.DistanceKm,
                c.AgeMin,
                c.AgeMax,
                c.GenderRestriction,
                c.EntryFee,
                c.StartTime
            })
        });
    }

    /// <summary>Organiser only - create a new event.</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var organiser = await _db.EventOrganisers.FirstOrDefaultAsync(o => o.UserId == CurrentUserId);
        if (organiser is null) return Forbidden();

        var ev = new Event
        {
            OrganiserId = organiser.OrganiserId,
            EventName = dto.EventName,
            EventType = dto.EventType,
            Description = dto.Description,
            DateTime = dto.DateTime,
            Location = dto.Location,
            MaxParticipants = dto.MaxParticipants,
            EntryFee = dto.EntryFee,
            RouteDescription = dto.RouteDescription,
            Status = dto.Status
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId, ev.EventName });
    }

    /// <summary>Organiser only - update own event.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound();

        if (ev.Organiser.UserId != CurrentUserId) return Forbidden();

        if (!string.IsNullOrWhiteSpace(dto.EventName)) ev.EventName = dto.EventName;
        if (!string.IsNullOrWhiteSpace(dto.EventType)) ev.EventType = dto.EventType;
        if (dto.Description is not null) ev.Description = dto.Description;
        if (dto.DateTime.HasValue) ev.DateTime = dto.DateTime.Value;
        if (!string.IsNullOrWhiteSpace(dto.Location)) ev.Location = dto.Location;
        if (dto.MaxParticipants.HasValue) ev.MaxParticipants = dto.MaxParticipants.Value;
        if (dto.EntryFee.HasValue) ev.EntryFee = dto.EntryFee.Value;
        if (dto.RouteDescription is not null) ev.RouteDescription = dto.RouteDescription;
        if (!string.IsNullOrWhiteSpace(dto.Status)) ev.Status = dto.Status;
        ev.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Event updated." });
    }

    /// <summary>Organiser only - delete own event.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound();
        if (ev.Organiser.UserId != CurrentUserId) return Forbidden();

        ev.IsArchived = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Public - list upcoming events (next 30 days).</summary>
    [HttpGet("upcoming")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventResponseDto>>> Upcoming()
    {
        var cutoff = DateTime.UtcNow.AddDays(30);
        var events = await _db.Events
            .Include(e => e.Organiser).Include(e => e.Categories)
            .Where(e => e.DateTime >= DateTime.UtcNow && e.DateTime <= cutoff && !e.IsArchived)
            .Select(e => new EventResponseDto(
                e.EventId, e.EventName, e.EventType, e.Description, e.DateTime,
                e.Location, e.MaxParticipants, e.EntryFee, e.RouteDescription, e.Status,
                e.OrganiserId, e.Organiser.OrganisationName, e.Categories.Count, e.CreatedAt))
            .ToListAsync();

        return Ok(events);
    }

    private ObjectResult Forbidden() => StatusCode(403, new { message = "You do not own this resource." });
}