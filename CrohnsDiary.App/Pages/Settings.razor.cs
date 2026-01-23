using CrohnsDiary.App.Database;
using CrohnsDiary.App.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace CrohnsDiary.App.Pages;

public partial class Settings
{
    private bool _showConsistency;
    private bool _showAmount;
    private bool _showEffort;
    private bool _showUrgency;
    private bool _showAir;

    [Inject]
    public required IStringLocalizer<Settings> Loc { get; set; }

    [Inject]
    public required ISettingsDatabase SettingsDatabase { get; set; }
    
    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private List<CustomMetric> CustomMetrics { get; set; } = new();
    
    private bool ShowMetricDialog { get; set; }
    private CustomMetric? EditingMetric { get; set; }
    private string EditingMetricName { get; set; } = string.Empty;
    private MetricType EditingMetricType { get; set; } = MetricType.Number;
    private int? EditingMetricMin { get; set; }
    private int? EditingMetricMax { get; set; }
    private int? EditingMetricDefault { get; set; }
    private string EditingMetricEnumValuesText { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _showConsistency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowConsistency, true);
        _showAmount = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAmount, true);
        _showEffort = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowEffort, true);
        _showUrgency = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowUrgency, true);
        _showAir = await SettingsDatabase.GetBoolValue(ISettingsDatabase.ShowAir, false);
        
        CustomMetrics = await SettingsDatabase.GetValue<List<CustomMetric>>(ISettingsDatabase.CustomMetrics) ?? new List<CustomMetric>();
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

    public bool ShowAir
    {
        get => _showAir;
        set
        {
            _showAir = value;
            Task.Run(() => SettingsDatabase.SaveValue(ISettingsDatabase.ShowAir, value));
        }
    }
    
    private void AddCustomMetric()
    {
        EditingMetric = null;
        EditingMetricName = string.Empty;
        EditingMetricType = MetricType.Number;
        EditingMetricMin = 1;
        EditingMetricMax = 5;
        EditingMetricDefault = 3;
        EditingMetricEnumValuesText = string.Empty;
        ShowMetricDialog = true;
    }
    
    private void EditCustomMetric(CustomMetric metric)
    {
        EditingMetric = metric;
        EditingMetricName = metric.Name;
        EditingMetricType = metric.Type;
        EditingMetricMin = metric.MinValue;
        EditingMetricMax = metric.MaxValue;
        EditingMetricDefault = metric.DefaultValue;
        EditingMetricEnumValuesText = metric.EnumValues.Any() ? string.Join(", ", metric.EnumValues) : string.Empty;
        ShowMetricDialog = true;
    }
    
    private async Task DeleteCustomMetric(CustomMetric metric)
    {
        CustomMetrics.Remove(metric);
        await SaveCustomMetrics();
    }
    
    private async Task SaveMetric()
    {
        // Validate metric name
        if (string.IsNullOrWhiteSpace(EditingMetricName))
        {
            Snackbar.Add(Loc["MetricNameRequired"], Severity.Error);
            return;
        }
        
        // Validate number metric
        if (EditingMetricType == MetricType.Number)
        {
            if (EditingMetricMin.HasValue && EditingMetricMax.HasValue && EditingMetricMin.Value > EditingMetricMax.Value)
            {
                Snackbar.Add(Loc["MinMaxValidation"], Severity.Error);
                return;
            }
            
            if (EditingMetricDefault.HasValue)
            {
                var min = EditingMetricMin ?? int.MinValue;
                var max = EditingMetricMax ?? int.MaxValue;
                if (EditingMetricDefault.Value < min || EditingMetricDefault.Value > max)
                {
                    Snackbar.Add(Loc["DefaultValueValidation"], Severity.Error);
                    return;
                }
            }
        }
        
        // Validate enum metric
        if (EditingMetricType == MetricType.Enum)
        {
            var enumValues = EditingMetricEnumValuesText
                .Split(',')
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
            
            if (enumValues.Count == 0)
            {
                Snackbar.Add(Loc["EnumValuesRequired"], Severity.Error);
                return;
            }
        }
        
        if (EditingMetric == null)
        {
            // Create new metric
            var newMetric = new CustomMetric
            {
                Id = Guid.NewGuid(),
                Name = EditingMetricName,
                Type = EditingMetricType,
                IsEnabled = true
            };
            
            if (EditingMetricType == MetricType.Number)
            {
                newMetric.MinValue = EditingMetricMin;
                newMetric.MaxValue = EditingMetricMax;
                newMetric.DefaultValue = EditingMetricDefault;
            }
            else if (EditingMetricType == MetricType.Enum)
            {
                newMetric.EnumValues = EditingMetricEnumValuesText
                    .Split(',')
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
            }
            
            CustomMetrics.Add(newMetric);
        }
        else
        {
            // Update existing metric
            EditingMetric.Name = EditingMetricName;
            EditingMetric.Type = EditingMetricType;
            
            if (EditingMetricType == MetricType.Number)
            {
                EditingMetric.MinValue = EditingMetricMin;
                EditingMetric.MaxValue = EditingMetricMax;
                EditingMetric.DefaultValue = EditingMetricDefault;
                EditingMetric.EnumValues.Clear();
            }
            else if (EditingMetricType == MetricType.Enum)
            {
                EditingMetric.EnumValues = EditingMetricEnumValuesText
                    .Split(',')
                    .Select(v => v.Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();
                EditingMetric.MinValue = null;
                EditingMetric.MaxValue = null;
                EditingMetric.DefaultValue = null;
            }
        }
        
        await SaveCustomMetrics();
        ShowMetricDialog = false;
        Snackbar.Add(Loc["MetricSaved"], Severity.Success);
    }
    
    private void CancelEditMetric()
    {
        ShowMetricDialog = false;
    }
    
    private async Task SaveCustomMetrics()
    {
        await SettingsDatabase.SaveValue(ISettingsDatabase.CustomMetrics, CustomMetrics);
        StateHasChanged();
    }
}
