namespace CrohnsDiary.App.Database;

public interface ISettingsDatabase
{
    public const string ShowConsistency = nameof(ShowConsistency);

    public const string ShowAmount = nameof(ShowAmount);

    public const string ShowUrgency = nameof(ShowUrgency);

    Task SaveValue<T>(string key, T value);
    Task<bool> GetBoolValue(string key, bool defaultValue);
}