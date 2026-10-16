using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class Participant
{
    public int ParticipantId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public string? MedicalConditions { get; set; }

    [MaxLength(200)]
    public string? ClubName { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}