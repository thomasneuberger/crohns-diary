namespace CrohnsDiary.App.Models;

public enum MetricType
{
    Number,
    Enum
}

public class CustomMetric
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required MetricType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Number type settings
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public int? DefaultValue { get; set; }
    
    // Enum type settings
    public List<string> EnumValues { get; set; } = new();
}
