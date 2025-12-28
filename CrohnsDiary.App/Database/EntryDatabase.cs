using BlazorDexie.Database;
using BlazorDexie.Options;
using CrohnsDiary.App.Models;

namespace CrohnsDiary.App.Database;

public class EntryDatabase(BlazorDexieOptions blazorDexieOptions)
    : Db<EntryDatabase>("EntryDatabase", 2, [], blazorDexieOptions)
{
    public Store<Entry, Guid> Entries { get; set; } = new(nameof(Entry.Id), nameof(Entry.Timestamp));
    public Store<BloodPressureEntry, Guid> BloodPressureEntries { get; set; } = new(nameof(BloodPressureEntry.Id), nameof(BloodPressureEntry.Timestamp));
}

