using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.AzureNative.Resources;
using AzureNative = Pulumi.AzureNative;
using Deployment = Pulumi.Deployment;
using SyncedFolder = Pulumi.SyncedFolder;

return await Deployment.RunAsync(() =>
{
    // Import the program's configuration settings.
    var config = new Config();
    var path = config.Get("path") ?? "./www";
    var indexDocument = config.Get("indexDocument") ?? "index.html";
    var errorDocument = config.Get("errorDocument") ?? "index.html";

    var stack = Deployment.Instance.StackName;
    var tags = new InputMap<string>();
    tags.Add("Stack", stack);

    // Create a resource group for the website.
    var resourceGroup = new AzureNative.Resources.ResourceGroup($"rg-crohns-diary-{stack}", new ResourceGroupArgs
    {
        Location = "westeurope",
        Tags = tags
    });

    // Create a blob storage account.
    var account = new AzureNative.Storage.StorageAccount($"stnbgcrohnd{stack}", new()
    {
        ResourceGroupName = resourceGroup.Name,
        Kind = "StorageV2",
        Sku = new AzureNative.Storage.Inputs.SkuArgs
        {
            Name = "Standard_LRS",
        },
        Tags = tags
    });

    // Configure the storage account as a website.
    var website = new AzureNative.Storage.StorageAccountStaticWebsite($"website-nbg-crohns-diary-{stack}", new()
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = account.Name,
        IndexDocument = indexDocument,
        Error404Document = errorDocument,
    });

    // Use a synced folder to manage the files of the website.
    var syncedFolder = new SyncedFolder.AzureBlobFolder("synced-folder", new()
    {
        Path = path,
        ResourceGroupName = resourceGroup.Name,
        StorageAccountName = account.Name,
        ContainerName = website.ContainerName,
    });

    // Create a CDN profile.
    var profile = new AzureNative.Cdn.Profile($"profile-nbg-crohns-diary-{stack}", new()
    {
        ResourceGroupName = resourceGroup.Name,
        Sku = new AzureNative.Cdn.Inputs.SkuArgs
        {
            Name = "Standard_Microsoft",
        },
        Tags = tags
    });

    // Pull the hostname out of the storage-account endpoint.
    var originHostname = account.PrimaryEndpoints.Apply(endpoints => new Uri(endpoints.Web).Host);

    // Create a CDN endpoint to distribute and cache the website.
    var endpoint = new AzureNative.Cdn.Endpoint($"endpoint-nbg-crohns-diary-{stack}", new()
    {
        ResourceGroupName = resourceGroup.Name,
        ProfileName = profile.Name,
        IsHttpAllowed = false,
        IsHttpsAllowed = true,
        IsCompressionEnabled = true,
        ContentTypesToCompress = new[]
        {
            "text/html",
            "text/css",
            "application/javascript",
            "application/json",
            "image/svg+xml",
            "font/woff",
            "font/woff2",
        },
        OriginHostHeader = originHostname,
        Origins = new[]
        {
            new AzureNative.Cdn.Inputs.DeepCreatedOriginArgs
            {
                Name = account.Name,
                HostName = originHostname,
            },
        },
        Tags = tags
    });

    // Export the URLs and hostnames of the storage account and CDN.
    return new Dictionary<string, object?>
    {
        ["originURL"] = account.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web),
        ["originHostname"] = originHostname,
        ["cdnURL"] = endpoint.HostName.Apply(hostName => $"https://{hostName}"),
        ["cdnHostname"] = endpoint.HostName,
    };
});
