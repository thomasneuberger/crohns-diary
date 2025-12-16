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

    protected override async Task OnInitializedAsync()
    {
        ShowConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        ShowAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
        ShowEffort = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowEffort, true);
        ShowUrgency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowUrgency, true);
        ShowAir = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAir, false);
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
        await Database.Entries.Add(entry, entry.Id);
        Snackbar.Add(Loc["Saved"], Severity.Success);
    }
}
