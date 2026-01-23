using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;

namespace CrohnsDiary.App.Pages;

public partial class Entries
{
    [Inject]
    public required EntryDatabase Database { get; set; }

    [Inject]
    public required NavigationManager Navigation { get; set; }
    
    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    private DateTime? SelectedDate { get; set; }

    private DateTime? EntriesListDate { get; set; }
    private IReadOnlyList<CombinedEntry> CombinedEntriesOnSelectedDate { get; set; } = [];
    
    private List<CustomMetric> CustomMetrics { get; set; } = new();

    protected override void OnInitialized()
    {
        SelectedDate = DateTime.Now.Date;
    }
    
    protected override async Task OnInitializedAsync()
    {
        CustomMetrics = await SettingsDatabase.GetValue<List<CustomMetric>>(ISettingsDatabase.CustomMetrics) ?? new List<CustomMetric>();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (SelectedDate.HasValue && EntriesListDate != SelectedDate)
        {
            await UpdateEntries(SelectedDate.Value);
        }
    }

    private async Task UpdateEntries(DateTime selectedDate)
    {
        var entries = await Database.Entries
            .Where(nameof(Entry.Timestamp))
            .Between(selectedDate, selectedDate.AddDays(1))
            .ToList();

        var bloodPressureEntries = await Database.BloodPressureEntries
            .Where(nameof(BloodPressureEntry.Timestamp))
            .Between(selectedDate, selectedDate.AddDays(1))
            .ToList();

        var combined = new List<CombinedEntry>();
        
        foreach (var entry in entries)
        {
            combined.Add(new CombinedEntry
            {
                Timestamp = entry.Timestamp,
                Type = "Entry",
                Consistency = entry.Consistency,
                Amount = entry.Amount,
                Effort = entry.Effort,
                Urgency = entry.Urgency,
                Air = entry.Air,
                CustomMetricValues = entry.CustomMetricValues
            });
        }

        foreach (var entry in bloodPressureEntries)
        {
            combined.Add(new CombinedEntry
            {
                Timestamp = entry.Timestamp,
                Type = "BloodPressure",
                Systolic = entry.Systolic,
                Diastolic = entry.Diastolic,
                PulseRate = entry.PulseRate
            });
        }

        CombinedEntriesOnSelectedDate = combined.OrderBy(e => e.Timestamp).ToList();
        EntriesListDate = selectedDate;

        StateHasChanged();
    }

    private Task NavigateToExportImport()
    {
        Navigation.NavigateTo("/export-import");

        return Task.CompletedTask;
    }
    
    public string GetCustomMetricValue(CombinedEntry entry, Guid metricId)
    {
        var value = entry.CustomMetricValues.FirstOrDefault(v => v.MetricId == metricId);
        if (value == null) return string.Empty;
        
        return value.NumberValue?.ToString() ?? value.EnumValue ?? string.Empty;
    }

    public class CombinedEntry
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        
        // Entry fields
        public int? Consistency { get; set; }
        public int? Amount { get; set; }
        public int? Effort { get; set; }
        public int? Urgency { get; set; }
        public int? Air { get; set; }
        
        // Blood pressure fields
        public int? Systolic { get; set; }
        public int? Diastolic { get; set; }
        public int? PulseRate { get; set; }
        
        // Custom metrics
        public List<CustomMetricValue> CustomMetricValues { get; set; } = new();
    }
}
