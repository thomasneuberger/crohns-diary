using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace CrohnsDiary.App.Pages;

public partial class DataProtection
{
    [Inject]
    public required HttpClient Http { get; set; }

    public string Content { get; set; } = """
                                          Loading...
                                          """;

    protected override async Task OnInitializedAsync()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var response = await Http.GetAsync($"/Dataprotection.{language}.md");
        if (response.IsSuccessStatusCode)
        {
            Content = await response.Content.ReadAsStringAsync();
        }
        else
        {
            Content = await Http.GetStringAsync("/Dataprotection.md");
        }
    }
}
