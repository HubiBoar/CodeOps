using Microsoft.EntityFrameworkCore;
using CodeOps.InfrastructureAsCode.Azure;
using Momolith.Modules;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CodeOps.ArgumentAsCode;
using Definit.Validation.FluentValidation;
using CodeOps.EnvironmentAsCode;

namespace Examples.WebApp.Modules.DataLayer;

public sealed class DataLayerModule : Module
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

    public sealed record MigrationArg(bool Value) : ArgAsCode.IArgument<MigrationArg, bool, IsNotNull<bool>>
    {
        public static string SectionName => "DataLayerMigration";

        public static string ArgumentShortcut => "cac";

        public static string ArgumentFullName => "config-as-code";

        public static MigrationArg Map(bool value) => new (value);
    }
}

public static class DataLayerModuleExtensions
{
    public static Task<DataLayerModule> Migrate(this Task<DataLayerModule> module, IHostExtender host, EnvAsCode.ModeArg shouldMigrate)
    {
        if(shouldMigrate.Value)
        {
            host.ExtendAsync(async host =>
            {
                var mod = await module;
                await mod.Migrate(host);
            });
        }

        return module;
    }

    public static Task<DataLayerModule> Migrate(this Task<DataLayerModule> module, IHostExtender host, DataLayerModule.MigrationArg shouldMigrate)
    {
        if(shouldMigrate.Value)
        {
            host.ExtendAsync(async host =>
            {
                var mod = await module;
                await mod.Migrate(host);
            });
        }

        return module;
    }

    public static DataLayerModule Migrate(this DataLayerModule module, IHostExtender host, EnvAsCode.ModeArg shouldMigrate)
    {
        if(shouldMigrate.Value)
        {
            host.ExtendAsync(module.Migrate);
        }

        return module;
    }

    public static DataLayerModule Migrate(this DataLayerModule module, IHostExtender host, DataLayerModule.MigrationArg shouldMigrate)
    {
        if(shouldMigrate.Value)
        {
            host.ExtendAsync(module.Migrate);
        }

        return module;
    }
}
