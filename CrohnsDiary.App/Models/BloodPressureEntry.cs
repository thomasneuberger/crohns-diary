namespace CrohnsDiary.App.Models;

public class BloodPressureEntry
{
    public required Guid Id { get; set; }
    public required DateTime Timestamp { get; set; }

    public int? Systolic { get; set; }

    public int? Diastolic { get; set; }

    public int? PulseRate { get; set; }
}
