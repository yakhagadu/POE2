using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.DTOs;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    /// <summary>Public - list all categories for a specific event.</summary>
    [HttpGet("events/{eventId}/categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetByEvent(int eventId)
    {
        var exists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!exists) return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryResponseDto(
                c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm,
                c.AgeMin, c.AgeMax, c.GenderRestriction, c.EntryFee, c.StartTime))
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Organiser only - create a new category for an event they own.</summary>
    [HttpPost("events/{eventId}/categories")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create(int eventId, CreateCategoryDto dto)
    {
        var ev = await _db.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.Organiser.UserId != CurrentUserId) return Forbidden();

        if (dto.AgeMin > dto.AgeMax)
            return BadRequest(new { message = "AgeMin cannot be greater than AgeMax." });

        var category = new Category
        {
            EventId = eventId,
            CategoryName = dto.CategoryName,
            DistanceKm = dto.DistanceKm,
            AgeMin = dto.AgeMin,
            AgeMax = dto.AgeMax,
            GenderRestriction = dto.GenderRestriction,
            EntryFee = dto.EntryFee,
            StartTime = dto.StartTime
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(201, new CategoryResponseDto(
            category.CategoryId, category.EventId, category.CategoryName,
            category.DistanceKm, category.AgeMin, category.AgeMax,
            category.GenderRestriction, category.EntryFee, category.StartTime));
    }

    /// <summary>Organiser only - update an own category.</summary>
    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories
            .Include(c => c.Event).ThenInclude(e => e.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event.Organiser.UserId != CurrentUserId) return Forbidden();

        if (!string.IsNullOrWhiteSpace(dto.CategoryName)) category.CategoryName = dto.CategoryName;
        if (dto.DistanceKm.HasValue) category.DistanceKm = dto.DistanceKm.Value;
        if (dto.AgeMin.HasValue) category.AgeMin = dto.AgeMin.Value;
        if (dto.AgeMax.HasValue) category.AgeMax = dto.AgeMax.Value;
        if (!string.IsNullOrWhiteSpace(dto.GenderRestriction)) category.GenderRestriction = dto.GenderRestriction;
        if (dto.EntryFee.HasValue) category.EntryFee = dto.EntryFee.Value;
        if (dto.StartTime.HasValue) category.StartTime = dto.StartTime.Value;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Category updated." });
    }

    /// <summary>Organiser only - delete an own category.</summary>
    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Event).ThenInclude(e => e.Organiser)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event.Organiser.UserId != CurrentUserId) return Forbidden();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private ObjectResult Forbidden() =>
        StatusCode(403, new { message = "You do not own this resource." });
}