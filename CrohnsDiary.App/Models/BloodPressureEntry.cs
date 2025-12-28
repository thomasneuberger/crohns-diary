namespace CrohnsDiary.App.Models;

public class BloodPressureEntry
{
    public required Guid Id { get; set; }
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Upper blood pressure value (systolic pressure)
    /// </summary>
    public int? Systolic { get; set; }

    /// <summary>
    /// Lower blood pressure value (diastolic pressure)
    /// </summary>
    public int? Diastolic { get; set; }

    public int? PulseRate { get; set; }
}
