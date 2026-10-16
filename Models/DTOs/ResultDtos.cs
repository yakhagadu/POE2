using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs;

public record CreateResultDto(
    [Required] TimeSpan FinishTime,
    int? Position,
    int? PositionCategory,
    decimal? Pace,
    string? Notes
);

public record UpdateResultDto(
    TimeSpan? FinishTime,
    int? Position,
    int? PositionCategory,
    decimal? Pace,
    string? Status,
    string? Notes
);

public record ResultResponseDto(
    int ResultId,
    int EnrolmentId,
    string ParticipantName,
    string EventName,
    TimeSpan FinishTime,
    int? Position,
    int? PositionCategory,
    decimal? Pace,
    string Status,
    string? Notes
);