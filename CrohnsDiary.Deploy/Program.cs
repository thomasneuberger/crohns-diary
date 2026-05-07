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

    // The Free SKU requires a linked repository at creation time. We set
    // SkipGithubActionWorkflowGeneration = true so Azure does not generate its own workflow
    // files — our CD workflow handles deployments via the static-web-apps-deploy action.
    //
    // Pulumi config is checked first (for local deployments); GitHub Actions env vars are
    // used as a fallback when running in CI.
    //   Local:   pulumi config set --secret CrohnsDiary:repositoryToken <github-pat>
    //            pulumi config set CrohnsDiary:repositoryUrl https://github.com/owner/repo
    //            pulumi config set CrohnsDiary:branch main
    //   CI:      GITHUB_TOKEN / GITHUB_REPOSITORY / GITHUB_REF_NAME are injected by the workflow.
    var config = new Config();
    var repositoryUrl = config.Get("repositoryUrl")
        ?? BuildGitHubRepositoryUrl();
    var repositoryToken = config.Get("repositoryToken")
        ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN")
        ?? throw new InvalidOperationException("Set 'repositoryToken' in Pulumi config (pulumi config set --secret CrohnsDiary:repositoryToken <token>) or ensure GITHUB_TOKEN env var is set.");
    var branch = config.Get("branch")
        ?? Environment.GetEnvironmentVariable("GITHUB_REF_NAME")
        ?? "main";

    // Create an Azure Static Web App.
    // RepositoryToken is only needed to satisfy the Free SKU create-time requirement;
    // IgnoreChanges prevents Pulumi from diffing/updating it on subsequent runs.
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
        RepositoryUrl = repositoryUrl,
        RepositoryToken = repositoryToken,
        Branch = branch,
        BuildProperties = new AzureNative.Web.Inputs.StaticSiteBuildPropertiesArgs
        {
            SkipGithubActionWorkflowGeneration = true,
        },
        Tags = tags
    }, new CustomResourceOptions
    {
        IgnoreChanges = { "repositoryToken" },
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

string BuildGitHubRepositoryUrl()
{
    var repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
        ?? throw new InvalidOperationException("Set 'repositoryUrl' in Pulumi config or ensure GITHUB_REPOSITORY env var is set.");
    return $"https://github.com/{repo}";
}
