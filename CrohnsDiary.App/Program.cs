using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CrohnsDiary.App;
using MudBlazor.Services;
using BlazorDexie.JsModule;
using CrohnsDiary.App.Database;
using BlazorDexie.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddDexieWrapper();
builder.Services.AddScoped<EntryDatabase>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

builder.Services.AddLocalization();

await builder.Build().RunAsync();
