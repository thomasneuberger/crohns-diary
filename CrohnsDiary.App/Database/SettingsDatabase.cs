using Blazored.LocalStorage;

namespace CrohnsDiary.App.Database;

public class SettingsDatabase(ILocalStorageService localStorageService) : ISettingsDatabase
{
    public async Task SaveValue<T>(string key, T value)
    {
        await localStorageService.SetItemAsync(key, value);
    }

    public async Task<bool> GetBoolValue(string key, bool defaultValue)
    {
        var value = await localStorageService.GetItemAsync<bool?>(key);
        return value ?? defaultValue;
    }
}
