using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public Participant Participant { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string PaymentStatus { get; set; } = "Pending";

    [MaxLength(50)]
    public string? PaymentReference { get; set; }

    public int? RaceNumber { get; set; }
    public int? StartWave { get; set; }

    [MaxLength(20)]
    public string EnrolmentStatus { get; set; } = "Active";

    public Result? Result { get; set; }
}