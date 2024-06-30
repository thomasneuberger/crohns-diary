using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.Cdn.Inputs;
using AzureNative = Pulumi.AzureNative;

namespace CrohnsDiary.Deploy;
internal class Cdn
{
    public Output<string> Url { get; init; }

    public Output<string> Hostname { get; init; }

    public Cdn(string stack, AzureNative.Resources.ResourceGroup resourceGroup, InputMap<string> tags, Input<string> originHostname, string? hostname)
    {
        // Create a CDN profile.
        var profile = new AzureNative.Cdn.Profile($"profile-nbg-crohns-diary-{stack}", new()
        {
            ProfileName = $"profile-nbg-crohns-diary-{stack}",
            ResourceGroupName = resourceGroup.Name,
            Sku = new AzureNative.Cdn.Inputs.SkuArgs
            {
                Name = "Standard_Microsoft",
            },
            Tags = tags
        });

        // Create a CDN endpoint to distribute and cache the website.
        var endpoint = new AzureNative.Cdn.Endpoint($"endpoint-nbg-crohns-diary-{stack}", new()
        {
            EndpointName = $"endpoint-nbg-crohns-diary-{stack}",
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
                    Name = originHostname.ToString()!.Split('.')[0],
                    HostName = originHostname,
                },
            },
            Tags = tags
        });

        if (hostname is not null)
        {
            var customDomain = new Pulumi.Azure.Cdn.EndpointCustomDomain("customDomain", new()
            {
                Name = hostname.Split('.')[0],
                CdnEndpointId = endpoint.Id,
                HostName = hostname,
                CdnManagedHttps = new EndpointCustomDomainCdnManagedHttpsArgs
                {
                    CertificateType= "Dedicated",
                    ProtocolType = "ServerNameIndication",
                    TlsVersion = "TLS12"
                }
            });
        }

        Url = hostname is not null ? Output<string>.Create(Task.FromResult(hostname)) : endpoint.HostName.Apply(endpointHostName => $"https://{endpointHostName}");
        Hostname = endpoint.HostName;
    }
}
