namespace CrohnsDiary.App.Database;

public interface ISettingsDatabase
{
    public const string ShowConsistency = nameof(ShowConsistency);

    public const string ShowAmount = nameof(ShowAmount);

    public const string ShowUrgency = nameof(ShowUrgency);

    public const string ShowEffort = nameof(ShowEffort);

    public const string ShowAir = nameof(ShowAir);

    public const string CustomMetrics = nameof(CustomMetrics);

    Task SaveValue<T>(string key, T value);
    Task<bool> GetBoolValue(string key, bool defaultValue);
    Task<T?> GetValue<T>(string key);
}