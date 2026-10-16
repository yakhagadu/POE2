using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public EventOrganiser Organiser { get; set; } = null!;

    [Required, MaxLength(200)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string EventType { get; set; } = string.Empty; // Running | Walking | Cycling

    public string? Description { get; set; }

    [Required]
    public DateTime DateTime { get; set; }

    [Required, MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
    public string? RouteDescription { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft | Open | Closed | Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsArchived { get; set; } = false;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}