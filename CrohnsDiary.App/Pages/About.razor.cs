using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace CrohnsDiary.App.Pages;

public partial class About
{
    [Inject]
    public required HttpClient Http { get; set; }

    public string Content { get; set; } = """
                                          Loading...
                                          """;

    protected override async Task OnInitializedAsync()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var response = await Http.GetAsync($"/About.{language}.md");
        if (response.IsSuccessStatusCode)
        {
            Content = await response.Content.ReadAsStringAsync();
        }
        else
        {
            Content = await Http.GetStringAsync("/About.md");
        }
    }
}
