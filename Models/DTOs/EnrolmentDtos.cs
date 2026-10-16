using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs;

public record CreateEnrolmentDto(
    [Required] int CategoryId,
    string? PaymentReference
);

public record EnrolmentResponseDto(
    int EnrolmentId,
    int ParticipantId,
    string ParticipantName,
    int CategoryId,
    string CategoryName,
    string EventName,
    DateTime RegistrationDate,
    string PaymentStatus,
    string EnrolmentStatus,
    int? RaceNumber
);