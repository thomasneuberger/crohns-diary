using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CrohnsDiary.App.Pages;

public partial class Home
{
    [Inject]
    public required ISnackbar Snackbar { get; set; }
    private DateTime? SelectedDate { get; set; } = DateTime.Now;

    private TimeSpan? SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    private void OnSave()
    {
        Snackbar.Add("Saved.", Severity.Success);
    }
}
