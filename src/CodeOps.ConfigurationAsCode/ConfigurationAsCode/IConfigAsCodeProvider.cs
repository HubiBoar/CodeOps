using OneOf;
using CodeOps.InfrastructureAsCode;
using Microsoft.Extensions.Configuration;

namespace CodeOps.ConfigurationAsCode;

public interface IConfigAsCodeProvider : InfraAsCode.IComponent
{
    public Task DownloadValues(IConfigurationBuilder builder);

    public Task UploadValues(IReadOnlyDictionary<string, OneOf<ConfigAsCode.Value, ConfigAsCode.Reference>> entries);

    public Task<bool> TryGetReference(string path, out string value);
}