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

    private bool _showConsistency;
    private bool _showAmount;

    public DateRange Range { get; set; } = new(DateTime.Today.AddMonths(-1), DateTime.Today);

    private ChartOptions Options { get; } = new ChartOptions
    {
        YAxisTicks = 1,
        YAxisRequireZeroPoint = true,
        YAxisLines = true
    };
    private string[] DayLabels { get; set; } = [];
    private List<ChartSeries> Series { get; } = new();

    private IReadOnlyList<DailyReport> dailyReports = [];

    private (DateOnly From, DateOnly To)? _loadedDateRange;

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        _showAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
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
                    .Average())
            .ToArray();
        Series.Add(new ChartSeries{Name = Loc["AverageUrgency"], Data = urgencies});

        dailyReports = dailyEntries
            .Select((d, index) => new DailyReport(d.Day)
            {
                Count = entryCounts[index],
                AverageConsistency = Math.Round(consistencies[index], 1),
                AverageUrgency = Math.Round(urgencies[index], 1)
            })
            .OrderBy(d => d.Day)
            .ToArray();
    }

    public struct DailyReport(DateOnly day)
    {
        public DateOnly Day { get; } = day;

        public double? Count { get; set; }

        public double? AverageConsistency { get; set; }

        public double? AverageUrgency { get; set; }
    }
}
