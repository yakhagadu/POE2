using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class User
{
    public int UserId { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty; // "Organiser" | "Participant"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public EventOrganiser? Organiser { get; set; }
    public Participant? Participant { get; set; }
}