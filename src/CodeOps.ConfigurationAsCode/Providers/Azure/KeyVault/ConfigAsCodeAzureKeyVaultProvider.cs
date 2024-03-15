using Azure.Core;
using Microsoft.Extensions.Configuration;
using Azure.Security.KeyVault.Secrets;
using OneOf;

namespace CodeOps.ConfigurationAsCode.Providers.Azure;

public sealed class ConfigAsCodeAzureKeyVaultProvider : IConfigAsCodeProvider
{
    private readonly Uri _keyVaultUri;
    private readonly TokenCredential _credential;
    private readonly SecretClient _secretClient;

    public ConfigAsCodeAzureKeyVaultProvider(Uri keyVaultUri, TokenCredential credential, SecretClient secretClient)
    {
        _keyVaultUri = keyVaultUri;
        _credential = credential;
        _secretClient = secretClient;
    }

    public Task DownloadValues(IConfigurationBuilder builder)
    {
        builder.AddAzureKeyVault(_keyVaultUri, _credential);

        return Task.CompletedTask;
    }

    public Task UploadValues(IReadOnlyDictionary<string, OneOf<ConfigAsCode.Value, ConfigAsCode.Reference>> entries)
    {
        foreach(var (name, entry) in entries)
        {
            entry.Switch(
                value =>
                {
                    _secretClient.SetSecret(name, value.Val);
                }, 
                reference =>
                {
                    if(TryGetReferenceInternal(reference.Path, out var referenceValue))
                    {
                        _secretClient.SetSecret(name, referenceValue);
                    }
                    else
                    {
                        throw new Exception($"KeyVaultReference '{name}' is not found in KV: {_keyVaultUri.AbsoluteUri}");
                    }
                });
        }

        return Task.CompletedTask;
    }

    public Task<bool> TryGetReference(string path, out string value)
    {
        return Task.FromResult(TryGetReferenceInternal(path, out value));
    }

    private bool TryGetReferenceInternal(string path, out string value)
    {
        var setting = _secretClient.GetSecret(path);

        if(setting is not null)
        {
            value = setting.Value.Value;
            return true;
        }
        else
        {
            value = string.Empty;
            return false;
        }    
    }
}