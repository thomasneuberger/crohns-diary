using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using ScottPlot.Blazor;

namespace CrohnsDiary.App.Pages;

public partial class Reports
{
    [Inject]
    public required EntryDatabase Database { get; set; }

    private BlazorPlot Plot { get; set; } = new();

    public DateRange Range { get; set; } = new(DateTime.Today.AddMonths(-1), DateTime.Today);

    private (DateOnly From, DateOnly To)? _loadedDateRange;

    protected override async Task OnAfterRenderAsync(bool firstRender)
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

        var days = dailyEntries
            .Select(d => d.Day.ToDateTime(TimeOnly.MinValue))
            .ToList();

        var entryCounts = dailyEntries
            .Select(d => d.Entries.Length)
            .ToList();
        Plot.Plot.Add.Scatter(days, entryCounts);
        Plot.Plot.Axes.DateTimeTicksBottom();
        Plot.Refresh();
    }
}
