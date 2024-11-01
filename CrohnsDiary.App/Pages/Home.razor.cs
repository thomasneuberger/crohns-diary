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

    private int Consistency { get; set; } = 3;

    private int Amount { get; set; } = 3;

    private int Effort { get; set; } = 3;

    public int Urgency { get; set; } = 3;

    private TimeSpan? SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    protected override async Task OnInitializedAsync()
    {
        ShowConsistency = await SettingsDatabase.GetValue(ISettingsDatabase.ShowConsistency, true);
    }

    private async Task OnSave()
    {
        var timestamp = SelectedDate.GetValueOrDefault(DateTime.Now).Date
            .Add(SelectedTime.GetValueOrDefault(DateTime.Now.TimeOfDay));
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Consistency = Consistency,
            Amount = Amount,
            Effort = Effort,
            Urgency = Urgency
        };
        await Database.Entries.Add(entry, entry.Id);
        Snackbar.Add(Loc["Saved"], Severity.Success);
    }
}
