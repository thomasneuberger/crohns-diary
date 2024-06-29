using Pulumi;
using System;
using AzureNative = Pulumi.AzureNative;
using SyncedFolder = Pulumi.SyncedFolder;

namespace CrohnsDiary.Deploy;
internal class Storage
{
    public Output<string> Url { get; init; }

    public Output<string> Hostname { get; init; }

    public Storage(string stack, Config config, AzureNative.Resources.ResourceGroup resourceGroup, InputMap<string> tags)
    {
        var path = config.Get("path") ?? "./www";
        var indexDocument = config.Get("indexDocument") ?? "index.html";
        var errorDocument = config.Get("errorDocument") ?? "index.html";

        // Create a blob storage account.
        var account = new AzureNative.Storage.StorageAccount($"stnbgcrohnd{stack}", new()
        {
            AccountName = $"stnbgcrohnd{stack}",
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

        Hostname = account.PrimaryEndpoints.Apply(endpoints => new Uri(endpoints.Web).Host);
        Url = account.PrimaryEndpoints.Apply(primaryEndpoints => primaryEndpoints.Web);
    }
}
