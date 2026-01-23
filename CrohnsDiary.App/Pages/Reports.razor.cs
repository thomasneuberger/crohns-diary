using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace CrohnsDiary.App.Pages;

public partial class Reports
{
    [Inject]
    public required EntryDatabase Database { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    [Inject]
    public required IStringLocalizer<Reports> Loc { get; set; }

    [Inject]
    public required IStringLocalizer<BloodPressureEntry> LocBloodPressure { get; set; }

    private bool _showConsistency;
    private bool _showUrgency;
    private bool _showAir;
    
    private List<CustomMetric> CustomMetrics { get; set; } = new();

    public DateRange Range { get; set; } = new(DateTime.Today.AddMonths(-1), DateTime.Today);

    private ChartOptions Options { get; } = new ChartOptions
    {
        YAxisTicks = 1,
        YAxisRequireZeroPoint = true,
        YAxisLines = true
    };
    private string[] DayLabels { get; set; } = [];
    private List<ChartSeries> Series { get; } = new();

    private ChartOptions BloodPressureOptions { get; } = new ChartOptions
    {
        YAxisTicks = 10,
        YAxisRequireZeroPoint = false,
        YAxisLines = true
    };
    private string[] BloodPressureDayLabels { get; set; } = [];
    private List<ChartSeries> BloodPressureSeries { get; } = new();

    private IReadOnlyList<DailyReport> dailyReports = [];

    private (DateOnly From, DateOnly To)? _loadedDateRange;

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        _showUrgency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowUrgency, true);
        _showAir = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAir, false);
        CustomMetrics = await SettingsDatabase.GetValue<List<CustomMetric>>(ISettingsDatabase.CustomMetrics) ?? new List<CustomMetric>();
        await FillChart();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await FillChart();
    }

    private async Task FillChart()
    {
        var from = DateOnly.FromDateTime(Range.Start!.Value);
        var to = DateOnly.FromDateTime(Range.End!.Value);
        if (_loadedDateRange is not null && _loadedDateRange.Value.From == from && _loadedDateRange.Value.To == to)
        {
            return;
        }

        var entries = await Database.Entries
            .Where(nameof(Entry.Timestamp))
            .Between(Range.Start!, Range.End!.Value.AddDays(1))
            .SortBy(nameof(Entry.Timestamp));

        var bloodPressureEntries = await Database.BloodPressureEntries
            .Where(nameof(BloodPressureEntry.Timestamp))
            .Between(Range.Start!, Range.End!.Value.AddDays(1))
            .SortBy(nameof(BloodPressureEntry.Timestamp));

        _loadedDateRange = (from, to);

        var dailyEntries = entries
            .GroupBy(e => DateOnly.FromDateTime(e.Timestamp.Date))
            .Select(d => new
            {
                Day = d.Key,
                Entries = d.ToArray()
            })
            .ToList();

        DayLabels = dailyEntries
            .Select(d => d.Day.ToShortDateString())
            .ToArray();

        Series.Clear();

        var entryCounts = dailyEntries
            .Select(d => (double)d.Entries.Length)
            .ToArray();
        Series.Add(new ChartSeries{Name = Loc["Count"], Data = entryCounts});

        var consistencies = dailyEntries
            .Select(d =>
                d.Entries
                    .Where(e => e.Consistency.HasValue)
                    .Select(e => (double)e.Consistency!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (_showConsistency)
        {
            Series.Add(new ChartSeries { Name = Loc["AverageConsistency"], Data = consistencies });
        }

        var urgencies = dailyEntries
            .Select(d =>
                d.Entries
                    .Where(e => e.Urgency.HasValue)
                    .Select(e => (double)e.Urgency!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (_showUrgency)
        {
            Series.Add(new ChartSeries { Name = Loc["AverageUrgency"], Data = urgencies });
        }

        var airs = dailyEntries
            .Select(d =>
                d.Entries
                    .Where(e => e.Air.HasValue)
                    .Select(e => (double)e.Air!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (_showAir)
        {
            Series.Add(new ChartSeries { Name = Loc["AverageAir"], Data = airs });
        }
        
        // Add custom number metrics to the chart
        foreach (var metric in CustomMetrics.Where(m => m.IsEnabled && m.Type == MetricType.Number))
        {
            var metricId = metric.Id;
            var customMetricValues = dailyEntries
                .Select(d =>
                    d.Entries
                        .SelectMany(e => e.CustomMetricValues)
                        .Where(v => v.MetricId == metricId && v.NumberValue.HasValue)
                        .Select(v => (double)v.NumberValue!.Value)
                        .DefaultIfEmpty(0)
                        .Average())
                .ToArray();
            
            if (customMetricValues.Any(v => v > 0))
            {
                Series.Add(new ChartSeries { Name = metric.Name, Data = customMetricValues });
            }
        }

        dailyReports = dailyEntries
            .Select((d, index) => new DailyReport(d.Day)
            {
                Count = entryCounts[index],
                AverageConsistency = Math.Round(consistencies[index], 1),
                AverageUrgency = Math.Round(urgencies[index], 1),
                AverageAir = Math.Round(airs[index], 1)
            })
            .OrderBy(d => d.Day)
            .ToArray();

        // Fill blood pressure chart
        var dailyBloodPressure = bloodPressureEntries
            .GroupBy(e => DateOnly.FromDateTime(e.Timestamp.Date))
            .Select(d => new
            {
                Day = d.Key,
                Entries = d.ToArray()
            })
            .ToList();

        BloodPressureDayLabels = dailyBloodPressure
            .Select(d => d.Day.ToShortDateString())
            .ToArray();

        BloodPressureSeries.Clear();

        var systolicValues = dailyBloodPressure
            .Select(d =>
                d.Entries
                    .Where(e => e.Systolic.HasValue)
                    .Select(e => (double)e.Systolic!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (systolicValues.Any(v => v > 0))
        {
            BloodPressureSeries.Add(new ChartSeries { Name = LocBloodPressure["Systolic"], Data = systolicValues });
        }

        var diastolicValues = dailyBloodPressure
            .Select(d =>
                d.Entries
                    .Where(e => e.Diastolic.HasValue)
                    .Select(e => (double)e.Diastolic!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (diastolicValues.Any(v => v > 0))
        {
            BloodPressureSeries.Add(new ChartSeries { Name = LocBloodPressure["Diastolic"], Data = diastolicValues });
        }

        var pulseRateValues = dailyBloodPressure
            .Select(d =>
                d.Entries
                    .Where(e => e.PulseRate.HasValue)
                    .Select(e => (double)e.PulseRate!.Value)
                    .DefaultIfEmpty(0)
                    .Average())
            .ToArray();
        if (pulseRateValues.Any(v => v > 0))
        {
            BloodPressureSeries.Add(new ChartSeries { Name = LocBloodPressure["PulseRate"], Data = pulseRateValues });
        }
    }

    public struct DailyReport(DateOnly day)
    {
        public DateOnly Day { get; } = day;

        public double? Count { get; set; }

        public double? AverageConsistency { get; set; }

        public double? AverageUrgency { get; set; }

        public double? AverageAir { get; set; }
    }
}
