using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs;

public record CreateCategoryDto(
    [Required] string CategoryName,
    [Range(0.1, 500)] decimal DistanceKm,
    int AgeMin = 10,
    int AgeMax = 120,
    string GenderRestriction = "Open",
    decimal EntryFee = 0,
    TimeSpan? StartTime = null
);

public record UpdateCategoryDto(
    string? CategoryName,
    decimal? DistanceKm,
    int? AgeMin,
    int? AgeMax,
    string? GenderRestriction,
    decimal? EntryFee,
    TimeSpan? StartTime
);

public record CategoryResponseDto(
    int CategoryId,
    int EventId,
    string CategoryName,
    decimal DistanceKm,
    int AgeMin,
    int AgeMax,
    string GenderRestriction,
    decimal EntryFee,
    TimeSpan? StartTime
);