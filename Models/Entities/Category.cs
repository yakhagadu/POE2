using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public decimal DistanceKm { get; set; }
    public int AgeMin { get; set; } = 10;
    public int AgeMax { get; set; } = 120;

    [MaxLength(10)]
    public string GenderRestriction { get; set; } = "Open";

    public decimal EntryFee { get; set; }
    public TimeSpan? StartTime { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}