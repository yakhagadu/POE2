using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string Role, // "Organiser" | "Participant"
                            // Organiser-specific
    string? OrganisationName,
    string? ContactPhone,
    string? Website,
    // Participant-specific
    DateTime? DateOfBirth,
    string? Gender,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? MedicalConditions,
    string? ClubName
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime ExpiresAt
);

public record UpdateProfileDto(
    string? FirstName,
    string? LastName,
    string? OrganisationName,
    string? ContactPhone,
    string? Website,
    string? Gender,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? MedicalConditions,
    string? ClubName
);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword
);