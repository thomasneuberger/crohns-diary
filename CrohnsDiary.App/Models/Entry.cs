namespace CrohnsDiary.App.Models;

public class Entry
{
    public required Guid Id { get; set; }
    public required DateTime Timestamp { get; set; }

    public int? Consistency { get; set; }

    public int? Amount { get; set; }

    public int? Effort { get; set; }

    public int? Urgency { get; set; }
}
