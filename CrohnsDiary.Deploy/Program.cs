using System;
using CrohnsDiary.Deploy;
using Pulumi;
using System.Collections.Generic;
using AzureNative = Pulumi.AzureNative;
using Deployment = Pulumi.Deployment;

return await Deployment.RunAsync(() =>
{
    // Import the program's configuration settings.
    var config = new Config();
    var hostname = config.Get("hostname")?.Trim();

    var stack = Deployment.Instance.StackName;
    var tags = new InputMap<string>();
    tags.Add("Stack", stack);

    // Create a resource group for the website.
    var resourceGroup = new AzureNative.Resources.ResourceGroup($"rg-crohns-diary-{stack}", new()
    {
        ResourceGroupName = $"rg-crohns-diary-{stack}",
        Location = "westeurope",
        Tags = tags
    });

    var storage = new Storage(stack, config, resourceGroup, tags);

    var cdn = new Cdn(stack, resourceGroup, tags, storage.Hostname, hostname);

    // Export the URLs and hostnames of the storage account and CDN.
    return new Dictionary<string, object?>
    {
        ["originURL"] = storage.Url,
        ["originHostname"] = storage.Hostname,
        ["cdnURL"] = cdn.Url,
        ["cdnHostname"] = cdn.Hostname,
    };
});
