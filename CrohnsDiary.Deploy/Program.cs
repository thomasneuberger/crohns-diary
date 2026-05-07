using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using AzureNative = Pulumi.AzureNative;
using Deployment = Pulumi.Deployment;

return await Deployment.RunAsync(async () =>
{
    var stack = Deployment.Instance.StackName;
    var tags = new InputMap<string>();
    tags.Add("Stack", stack);

    var resourceGroupName = $"rg-crohns-diary-{stack}";

    // Create a resource group for the website.
    var resourceGroup = new AzureNative.Resources.ResourceGroup($"rg-crohns-diary-{stack}", new ResourceGroupArgs
    {
        ResourceGroupName = resourceGroupName,
        Location = "westeurope",
        Tags = tags
    });

    var staticWebAppName = $"stapp-crohns-diary-{stack}";

    // Create an Azure Static Web App.
    var staticSite = new AzureNative.Web.StaticSite(staticWebAppName, new()
    {
        Name = staticWebAppName,
        ResourceGroupName = resourceGroup.Name,
        Location = "westeurope",
        Sku = new AzureNative.Web.Inputs.SkuDescriptionArgs
        {
            Name = "Free",
            Tier = "Free",
        },
        Tags = tags
    });

    await WriteOutputVariable("RESOURCE_GROUP_NAME", resourceGroupName);
    await WriteOutputVariable("STATIC_WEB_APP_NAME", staticWebAppName);

    // Export the URL and hostname of the static web app.
    return new Dictionary<string, object?>
    {
        ["staticWebAppURL"] = staticSite.DefaultHostname.Apply(hostName => $"https://{hostName}"),
        ["staticWebAppHostname"] = staticSite.DefaultHostname,
    };
});

async Task WriteOutputVariable(string name, string value)
{
    var path = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (File.Exists(path))
    {
        await File.AppendAllLinesAsync(path, [$"{name}={value}"]);
    }
}
