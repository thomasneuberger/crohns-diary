using CrohnsDiary.App.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace CrohnsDiary.App.Pages;

public partial class Settings
{
    private bool _showConsistency;
    private bool _showAmount;
    private bool _showEffort;
    private bool _showUrgency;

    [Inject]
    public required IStringLocalizer<Settings> Loc { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        _showAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
        _showEffort = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowEffort, true);
        _showUrgency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowUrgency, true);
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

    public bool ShowEffort
    {
        get => _showEffort;
        set
        {
            _showEffort = value;
            Task.Run(() => SettingsDatabase.SaveValue(ISettingsDatabase.ShowEffort, value));
        }
    }

    public bool ShowUrgency
    {
        get => _showUrgency;
        set
        {
            _showUrgency = value;
            Task.Run(() => SettingsDatabase.SaveValue(ISettingsDatabase.ShowUrgency, value));
        }
    }
}
