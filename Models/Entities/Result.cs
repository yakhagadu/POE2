namespace RaceDay.API.Models.Entities;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    public TimeSpan FinishTime { get; set; }
    public int? Position { get; set; }
    public int? PositionCategory { get; set; }
    public decimal? Pace { get; set; }

    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }

    public int? VerifiedBy { get; set; }
    public DateTime VerificationDate { get; set; } = DateTime.UtcNow;
}