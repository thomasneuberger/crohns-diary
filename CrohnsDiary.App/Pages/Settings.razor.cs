using CrohnsDiary.App.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace CrohnsDiary.App.Pages;

public partial class Settings
{
    private bool _showConsistency;
    private bool _showAmount;

    [Inject]
    public required IStringLocalizer<Settings> Loc { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        _showAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
    }

    public bool ShowConsistency
    {
        get => _showConsistency;
        set
        {
            _showConsistency = value;
            Task.Run(() => SettingsDatabase.SaveValue(ISettingsDatabase.ShowConsistency, value));
        }
    }

    public bool ShowAmount
    {
        get => _showAmount;
        set
        {
            _showAmount = value;
            Task.Run(() => SettingsDatabase.SaveValue(ISettingsDatabase.ShowAmount, value));
        }
    }
}
