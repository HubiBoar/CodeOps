using Microsoft.Extensions.Configuration;
using Azure.Security.KeyVault.Secrets;
using OneOf;
using Microsoft.Extensions.DependencyInjection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

namespace CodeOps.ConfigurationAsCode.Azure;

public sealed class AzureKeyVaultConfigAsCodeSource : ConfigAsCode.ISource
{
    private readonly SecretClient _secretClient;

    public AzureKeyVaultConfigAsCodeSource(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }

    public void UploadValues(IReadOnlyDictionary<ConfigAsCode.Path, OneOf<ConfigAsCode.Value, ConfigAsCode.Reference>> entries)
    {
        foreach(var (path, entry) in entries)
        {
            entry.Switch(
                value =>
                {
                    _secretClient.SetSecret(path.Value, value.Val);
                }, 
                reference =>
                {
                    var referenceValue = GetValue(reference.Path);
                    _secretClient.SetSecret(path.Value, referenceValue);
                });
        }
    }

    public string GetValue(ConfigAsCode.Path path)
    {
        var setting = _secretClient.GetSecret(path.Value);

        return setting?.Value?.Value ?? string.Empty;        
    }

    public void Register(IServiceCollection services, IConfigurationManager config)
    {
        config.AddAzureKeyVault(_secretClient, new KeyVaultSecretManager());
    }

    public void UploadValues(
        IReadOnlyDictionary<
            ConfigAsCode.Path,
            OneOf<
                ConfigAsCode.Value, 
                ConfigAsCode.FiltersEnabledFor,
                ConfigAsCode.FeatureFlag,
                ConfigAsCode.Reference>>
        entries)
    {
        Dictionary<ConfigAsCode.Path, ConfigAsCode.Reference> references = [];

        foreach(var (path, entry) in entries)
        {
            entry.Switch(
                value     => _secretClient.SetSecret(path.Value, value.Val),
                filter    => _secretClient.SetSecret(path.Value, filter.ToJson()),
                flag      => _secretClient.SetSecret(path.Value, flag.ToJson()),
                reference => references.Add(path, reference));
        }

        foreach(var (path, reference) in references)
        {
            _secretClient.SetSecret(path.Value, GetValue(reference.Path));
        }
    }
}