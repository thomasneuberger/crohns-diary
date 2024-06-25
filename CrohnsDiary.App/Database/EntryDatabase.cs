using BlazorDexie.Database;
using BlazorDexie.JsModule;
using CrohnsDiary.App.Models;

namespace CrohnsDiary.App.Database;

public class EntryDatabase(
    IModuleFactory jsModuleFactory)
    : Db("EntryDatabase", 1, Array.Empty<DbVersion>(), jsModuleFactory)
{
    public Store<Entry, Guid> Entries { get; set; } = new(nameof(Entry.Id), nameof(Entry.Timestamp));
}

