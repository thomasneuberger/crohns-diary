using Blazored.LocalStorage;

namespace CrohnsDiary.App.Database;

public class SettingsDatabase(ILocalStorageService localStorageService) : ISettingsDatabase
{
    public async Task SaveValue<T>(string key, T value)
    {
        await localStorageService.SetItemAsync(key, value);
    }

    public async Task<T> GetValue<T>(string key, T defaultValue)
    {
        return await localStorageService.GetItemAsync<T>(key) ?? defaultValue;
    }
}
