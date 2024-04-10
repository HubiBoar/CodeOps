using Examples.WebApp;
using Momolith.Modules;
using Examples.WebApp.Modules.DataLayer;
using CodeOps.InfrastructureAsCode;
using Environment = Examples.WebApp.Environment;
using CodeOps.InfrastructureAsCode.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var environment = new Environment(builder, args);

var sql = environment.CreateComponent<SqlServerConnection>(environment);

builder.AddModule(setup => DataLayerModule.Create(setup));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();