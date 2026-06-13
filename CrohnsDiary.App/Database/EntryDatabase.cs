﻿using Microsoft.JSInterop;
using CrohnsDiary.App.Models;

namespace CrohnsDiary.App.Database;

public class EntryDatabase(IJSRuntime jsRuntime)
{
    public LocalStore<Entry, Guid> Entries { get; } = new(jsRuntime, "EntryDatabase", "Entries");
    public LocalStore<BloodPressureEntry, Guid> BloodPressureEntries { get; } = new(jsRuntime, "EntryDatabase", "BloodPressureEntries");
}

