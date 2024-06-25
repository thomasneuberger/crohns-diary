using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CrohnsDiary.App.Pages;

public partial class Home
{
    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required EntryDatabase Database { get; set; }

    private DateTime? SelectedDate { get; set; } = DateTime.Now;

    private TimeSpan? SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    private async Task OnSave()
    {
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Timestamp = SelectedDate.GetValueOrDefault(DateTime.Now).Date
                .Add(SelectedTime.GetValueOrDefault(DateTime.Now.TimeOfDay))
        };
        await Database.Entries.Add(entry, entry.Id);
        Snackbar.Add("Saved.", Severity.Success);
    }
}
