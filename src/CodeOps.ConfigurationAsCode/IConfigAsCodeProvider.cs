namespace CodeOps.ConfigurationAsCode;

public interface IConfigAsCodeProvider
{
    public Task<IReadOnlyDictionary<string, string>> GetValues();

    public Task UploadValues(IReadOnlyDictionary<string, string> entries);
}