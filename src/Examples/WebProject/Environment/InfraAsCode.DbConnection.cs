using CodeOps.ArgumentAsCode;
using CodeOps.InfrastructureAsCode;
using CodeOps.InfrastructureAsCode.Azure;
using CodeOps.EnvironmentAsCode;
using Azure.ResourceManager.Sql.Models;
using Sql = CodeOps.InfrastructureAsCode.Azure.AzureSqlProvider;

namespace Examples.WebProject;

internal sealed partial class Environment :
    InfraAsCode.IEntry<DBConnectionComponent>
{
    public InfraAsCode.Entry<DBConnectionComponent> InfrastructureAsCode(InfraAsCode.Context<DBConnectionComponent> context)
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

        InfraAsCode.Entry<DBConnectionComponent> Create(Sql.ServerName serverName, Sql.DbName dbName, SqlSku sku)
        {
            return new Sql(serverName, dbName, sku, _ => {})
                .InfraAsCode(this.GetArgument<AzureOptions>(), context);
        }
    }
}