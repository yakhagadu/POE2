using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class EventOrganiser
{
    public int OrganiserId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string OrganisationName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;

    public string? Website { get; set; }
    public bool VerifiedStatus { get; set; } = false;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}