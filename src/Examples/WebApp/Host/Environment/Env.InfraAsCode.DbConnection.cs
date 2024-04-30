using CodeOps.ArgumentAsCode;
using CodeOps.InfrastructureAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using Azure.ResourceManager.Sql.Models;
using Sql = CodeOps.InfrastructureAsCode.Azure.AzureSqlProvider;

namespace Examples.WebApp;

internal sealed partial class Environment :
    InfraAsCode.IEntry<SqlServerConnection>
{
    public InfraAsCode.Entry<SqlServerConnection> InfrastructureAsCode(InfraAsCode.Context<SqlServerConnection> context)
    {
        return MatchEnvironment(
            prod =>
                Create(
                    new Sql.ServerName("SQL-P"),
                    new Sql.DbName("sql-db"),
                    new SqlSku("Standard")),
            test =>
                Create(
                    new Sql.ServerName("SQL-T"),
                    new Sql.DbName("sql-db"),
                    new SqlSku("Basic")));

        InfraAsCode.Entry<SqlServerConnection> Create(Sql.ServerName serverName, Sql.DbName dbName, SqlSku sku)
        {
            return new Sql(serverName, dbName, sku, _ => {})
                .InfraAsCode(GetAzureOptions(), context);
        }
    }
}