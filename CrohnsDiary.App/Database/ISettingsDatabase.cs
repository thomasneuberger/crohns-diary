namespace CrohnsDiary.App.Database;

public interface ISettingsDatabase
{
    public const string ShowConsistency = nameof(ShowConsistency);

    Task SaveValue<T>(string key, T value);
    Task<T> GetValue<T>(string key, T defaultValue);
}