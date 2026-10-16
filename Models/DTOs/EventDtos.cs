using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs;

public record CreateEventDto(
    [Required, MaxLength(200)] string EventName,
    [Required] string EventType,
    string? Description,
    [Required] DateTime DateTime,
    [Required] string Location,
    [Range(1, int.MaxValue)] int MaxParticipants,
    decimal EntryFee,
    string? RouteDescription,
    string Status = "Draft"
);

public record UpdateEventDto(
    string? EventName,
    string? EventType,
    string? Description,
    DateTime? DateTime,
    string? Location,
    int? MaxParticipants,
    decimal? EntryFee,
    string? RouteDescription,
    string? Status
);

public record EventResponseDto(
    int EventId,
    string EventName,
    string EventType,
    string? Description,
    DateTime DateTime,
    string Location,
    int MaxParticipants,
    decimal EntryFee,
    string? RouteDescription,
    string Status,
    int OrganiserId,
    string OrganiserName,
    int CategoryCount,
    DateTime CreatedAt
);