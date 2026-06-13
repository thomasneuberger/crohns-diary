using System.Reflection;
using Microsoft.JSInterop;

namespace CrohnsDiary.App.Database;

public class LocalStore<TItem, TKey>(IJSRuntime jsRuntime, string databaseName, string storeName)
    where TKey : notnull
{
    private const string ScriptNamespace = "crohnsDiaryIndexedDb";

    public async Task Add(TItem item)
    {
        await jsRuntime.InvokeVoidAsync($"{ScriptNamespace}.add", databaseName, storeName, item);
    }

    public async Task BulkPut(IEnumerable<TItem> items)
    {
        await jsRuntime.InvokeVoidAsync($"{ScriptNamespace}.bulkPut", databaseName, storeName, items);
    }

    public async Task<List<TItem>> ToList()
    {
        var items = await jsRuntime.InvokeAsync<List<TItem>>($"{ScriptNamespace}.getAll", databaseName, storeName);
        return items ?? [];
    }

    public LocalStoreQuery<TItem, TKey> Where(string propertyName)
    {
        return new(this, propertyName);
    }

    public async Task<List<TItem>> SortBy(string propertyName)
    {
        var property = ResolveProperty(propertyName);
        var items = await ToList();

        return items
            .OrderBy(item => property.GetValue(item) as IComparable)
            .ToList();
    }

    internal PropertyInfo ResolveProperty(string propertyName)
    {
        var property = typeof(TItem).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found on type '{typeof(TItem).Name}'.");
        }

        return property;
    }
}

public class LocalStoreQuery<TItem, TKey>(LocalStore<TItem, TKey> store, string propertyName)
    where TKey : notnull
{
    private readonly List<Func<TItem, bool>> _predicates = [];

    public LocalStoreQuery<TItem, TKey> Between(object lowerInclusive, object upperExclusive)
    {
        var property = store.ResolveProperty(propertyName);
        _predicates.Add(item =>
        {
            var value = property.GetValue(item) as IComparable;
            if (value is null)
            {
                return false;
            }

            return value.CompareTo(lowerInclusive) >= 0 && value.CompareTo(upperExclusive) < 0;
        });

        return this;
    }

    public async Task<List<TItem>> ToList()
    {
        var items = await store.ToList();

        return items
            .Where(item => _predicates.All(predicate => predicate(item)))
            .ToList();
    }

    public async Task<List<TItem>> SortBy(string sortPropertyName)
    {
        var sortProperty = store.ResolveProperty(sortPropertyName);
        var filtered = await ToList();

        return filtered
            .OrderBy(item => sortProperty.GetValue(item) as IComparable)
            .ToList();
    }
}
