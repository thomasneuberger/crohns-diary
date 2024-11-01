using CrohnsDiary.App.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace CrohnsDiary.App.Pages;

public partial class Settings
{
    private bool _showConsistency;

    [Inject]
    public required IStringLocalizer<Settings> Loc { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    public bool ShowConsistency
    {
        get => _showConsistency;
        set
        {
            _showConsistency = value;
            Task.Run(OnShowConsistencyChanged);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetValue(ISettingsDatabase.ShowConsistency, true);
    }

    private async Task OnShowConsistencyChanged()
    {
        await SettingsDatabase.SaveValue(ISettingsDatabase.ShowConsistency, ShowConsistency);
    }
}
