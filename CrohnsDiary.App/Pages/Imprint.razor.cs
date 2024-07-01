using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace CrohnsDiary.App.Pages;

public partial class Imprint
{
    [Inject]
    public required HttpClient Http { get; set; }

    public string Content { get; set; } = """
                                          Loading...
                                          """;

    protected override async Task OnInitializedAsync()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var response = await Http.GetAsync($"/Imprint.{language}.md");
        if (response.IsSuccessStatusCode)
        {
            Content = await response.Content.ReadAsStringAsync();
        }
        else
        {
            Content = await Http.GetStringAsync("/Imprint.md");
        }
    }
}
