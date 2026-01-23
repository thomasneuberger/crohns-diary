using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace CrohnsDiary.App.Pages;

public partial class Home
{
    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required EntryDatabase Database { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    [Inject]
    public required IStringLocalizer<Home> Loc { get; set; }

    private DateTime? SelectedDate { get; set; } = DateTime.Now;

    private bool ShowConsistency { get; set; }

    private bool ShowAmount { get; set; }

    private bool ShowEffort { get; set; }

    private bool ShowUrgency { get; set; }

    private bool ShowAir { get; set; }

    private int Consistency { get; set; } = 3;

    private int Amount { get; set; } = 3;

    private int Effort { get; set; } = 3;

    public int Urgency { get; set; } = 3;

    private int Air { get; set; } = 0;

    private TimeSpan? SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    // Blood Pressure fields
    private DateTime? BloodPressureDate { get; set; } = DateTime.Now;
    private TimeSpan? BloodPressureTime { get; set; } = DateTime.Now.TimeOfDay;
    private int Systolic { get; set; } = 120;
    private int Diastolic { get; set; } = 80;
    private int PulseRate { get; set; } = 70;
    
    // Custom metrics
    private List<CustomMetric> CustomMetrics { get; set; } = new();
    private Dictionary<Guid, int> CustomNumberValues { get; set; } = new();
    private Dictionary<Guid, string> CustomEnumValues { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        ShowConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        ShowAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
        ShowEffort = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowEffort, true);
        ShowUrgency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowUrgency, true);
        ShowAir = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAir, false);
        
        CustomMetrics = await SettingsDatabase.GetValue<List<CustomMetric>>(ISettingsDatabase.CustomMetrics) ?? new List<CustomMetric>();
        
        // Initialize custom metric values with defaults
        foreach (var metric in CustomMetrics.Where(m => m.IsEnabled))
        {
            if (metric.Type == MetricType.Number && metric.DefaultValue.HasValue)
            {
                CustomNumberValues[metric.Id] = metric.DefaultValue.Value;
            }
            else if (metric.Type == MetricType.Enum && metric.EnumValues.Any())
            {
                CustomEnumValues[metric.Id] = metric.EnumValues.First();
            }
        }
    }

    private async Task OnSave()
    {
        var timestamp = SelectedDate.GetValueOrDefault(DateTime.Now).Date
            .Add(SelectedTime.GetValueOrDefault(DateTime.Now.TimeOfDay));
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Consistency = ShowConsistency ? Consistency : null,
            Amount = ShowAmount ? Amount : null,
            Effort = ShowEffort ? Effort : null,
            Urgency = ShowUrgency ? Urgency : null,
            Air = ShowAir ? Air : null
        };
        
        // Add custom metric values
        foreach (var metric in CustomMetrics.Where(m => m.IsEnabled))
        {
            if (metric.Type == MetricType.Number && CustomNumberValues.ContainsKey(metric.Id))
            {
                entry.CustomMetricValues.Add(new CustomMetricValue
                {
                    MetricId = metric.Id,
                    NumberValue = CustomNumberValues[metric.Id]
                });
            }
            else if (metric.Type == MetricType.Enum && CustomEnumValues.ContainsKey(metric.Id))
            {
                entry.CustomMetricValues.Add(new CustomMetricValue
                {
                    MetricId = metric.Id,
                    EnumValue = CustomEnumValues[metric.Id]
                });
            }
        }
        
        await Database.Entries.Add(entry, entry.Id);
        Snackbar.Add(Loc["Saved"], Severity.Success);
    }

    private async Task OnSaveBloodPressure()
    {
        var timestamp = BloodPressureDate.GetValueOrDefault(DateTime.Now).Date
            .Add(BloodPressureTime.GetValueOrDefault(DateTime.Now.TimeOfDay));
        var entry = new BloodPressureEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Systolic = Systolic,
            Diastolic = Diastolic,
            PulseRate = PulseRate
        };
        await Database.BloodPressureEntries.Add(entry, entry.Id);
        Snackbar.Add(Loc["Saved"], Severity.Success);
    }
}
