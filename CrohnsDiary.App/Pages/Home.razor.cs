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
    public required IStringLocalizer<Home> Loc { get; set; }

    private DateTime? SelectedDate { get; set; } = DateTime.Now;

    private int Consistency { get; set; } = 3;

    private int Amount { get; set; } = 3;

    private TimeSpan? SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    private async Task OnSave()
    {
        var timestamp = SelectedDate.GetValueOrDefault(DateTime.Now).Date
            .Add(SelectedTime.GetValueOrDefault(DateTime.Now.TimeOfDay));
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Consistency = Consistency,
            Amount = Amount
        };
        await Database.Entries.Add(entry, entry.Id);
        Snackbar.Add(Loc["Saved"], Severity.Success);
    }
}
