namespace CrohnsDiary.App.Models;

public class CustomMetricValue
{
    public required Guid MetricId { get; set; }
    public int? NumberValue { get; set; }
    public string? EnumValue { get; set; }
}
