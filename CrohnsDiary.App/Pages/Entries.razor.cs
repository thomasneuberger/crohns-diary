using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;

namespace CrohnsDiary.App.Pages;

public partial class Entries
{
    [Inject]
    public required EntryDatabase Database { get; set; }

    private DateTime? SelectedDate { get; set; }

    private DateTime? EntriesListDate { get; set; }
    private IReadOnlyList<Entry> EntriesOnSelectedDate { get; set; } = [];

    protected override void OnInitialized()
    {
        SelectedDate = DateTime.Now.Date;
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
        EntriesOnSelectedDate = await Database.Entries
            .Where(nameof(Entry.Timestamp))
            .Between(selectedDate, selectedDate.AddDays(1))
            .ToList();

        EntriesListDate = selectedDate;

        StateHasChanged();
    }
}
