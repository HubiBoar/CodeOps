using Microsoft.EntityFrameworkCore;
using CodeOps.InfrastructureAsCode.Azure;
using Momolith.Modules;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.WebApp.Modules.DataLayer;

public class DataLayerModule : Module
{
    private readonly List<Func<IHost, Task>> _migrations;
    private readonly Action<DbContextOptionsBuilder>? _optionsAction;

    private DataLayerModule(
        ModuleSetup moduleSetup,
        Action<DbContextOptionsBuilder>? optionsAction)
        : base(moduleSetup)
    {
        _optionsAction = optionsAction;
        _migrations = [];
    }

    public static DataLayerModule Create(ModuleSetup setup, SqlServerConnection sqlConnectionString)
    {
        return new DataLayerModule(setup, options => options.UseSqlServer(sqlConnectionString.ConnectionString));
    }

    public void AddDbContextFactory<T>()
        where T : DbContext
    {
        Services.AddDbContextFactory<T>(_optionsAction);
        _migrations.Add(Migrate<T>);
    }

    public Task Migrate(IHost host)
    {
        return Task.WhenAll(_migrations.Select(x => x.Invoke(host)));
    }

    private static async Task Migrate<T>(IHost host)
        where T : DbContext
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();

        await context.Database.MigrateAsync();
    }
}
