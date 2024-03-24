using CodeOps.InfrastructureAsCode;
using CliWrap;

namespace CodeOps.DeploymentAsCode;

public sealed record ContainerHub(string Name, Uri Uri, string DockerLoginCommand) : InfraAsCode.IComponent
{
    //s: 2008-06-15T21:15:07
    public static string GenerateTag() => DateTime.Now.ToString("s");

    public async Task<(string latestTag, string tagValue)> DeployCode(string containerName)
    {
        var tag = GenerateTag();
        var latestTag = "latest";

        await DeployCode(containerName, [ tag, latestTag ]);

        return (latestTag, tag);
    }

    public async Task DeployCode(string containerName, IReadOnlyCollection<string> containerTags)
    {
        try
        {
            var tags = containerTags.Count > 1 ? $"ContainerImageTags={string.Join(";", containerTags)}" : $"ContainerImageTag={containerTags.First()}";

            await Cli.Wrap("dotnet")
                .WithArguments($"publish --os linux --arch x64 -p:DockerDefaultTargetOS=Linux -p:PublishProfile=DefaultContainer -p:ContainerImageName={Uri.Host}/{containerName}/{tags} -p: -c Release")
                .ExecuteAsync();
            
            await Cli.Wrap("docker")
                .WithArguments(DockerLoginCommand)
                .ExecuteAsync();

            await Cli.Wrap("docker")
                .WithArguments($"push {Uri.Host}/{containerName} --all-tags")
                .ExecuteAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}