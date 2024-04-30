using Examples.WebApp;
using Momolith.Modules;
using Examples.WebApp.Modules.DataLayer;
using CodeOps.InfrastructureAsCode;
using Environment = Examples.WebApp.Environment;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using CodeOps.ArgumentAsCode;

var builder = WebApplication.CreateBuilder(args);

var environment = new Environment(builder, args);
var mode = environment.GetArgument<EnvAsCode.ModeArg>();

var hostExtender = new WebAppExtender();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sql = await environment.CreateComponent<SqlServerConnection>(environment);

builder.AddModule(setup => DataLayerModule.Create(setup, sql))
    .Migrate(hostExtender, mode);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await hostExtender.RunExtensionsAsync(app);

app.UseHttpsRedirection();

app.Run();