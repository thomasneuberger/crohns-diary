using BlazorDexie.Extensions;
using Blazored.LocalStorage;
using CrohnsDiary.App;
using CrohnsDiary.App.Database;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorDexie();
builder.Services.AddScoped<EntryDatabase>();
builder.Services.AddScoped<ISettingsDatabase, SettingsDatabase>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLocalization();

await builder.Build().RunAsync();
