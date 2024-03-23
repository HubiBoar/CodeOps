using Azure;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;

namespace CodeOps.InfrastructureAsCode.Azure;

public sealed record DBConnectionComponent(string ConnectionString) : InfraAsCode.IComponent;

public sealed class AzureSqlProvider : IAzureComponentProvider<DBConnectionComponent>
{
    public sealed record ServerName(string Value);
    public sealed record DbName(string Value);

    private readonly ServerName _serverName;
    private readonly DbName _dbName;
    private readonly SqlSku _sku;
    private readonly Action<SqlServerData> _modifyServer;

    public AzureSqlProvider(ServerName serverName, DbName dbName, SqlSku sku, Action<SqlServerData> modifyServer)
    {
        _dbName = dbName;
        _serverName = serverName;
        _sku = sku;
        _modifyServer = modifyServer;
    }
    
    public DBConnectionComponent Provision(AzureCredentails credentials, AzureDeploymentOptions options)
    {
        var subscription = options.Subscription;
        var location = options.Location;
        var resourceGroup = options.ResourceGroup;
      
        var serverData = new SqlServerData(location)
        {
            Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)
        };

        _modifyServer(serverData);

        var serverResult = resourceGroup
            .GetSqlServers()
            .CreateOrUpdate(
                WaitUntil.Completed,
                _serverName.Value,
                serverData)
            .Value;
            
        var dataBaseData = new SqlDatabaseData(location)
        {
            Sku = _sku
        };

        serverResult
            .GetSqlDatabases()
            .CreateOrUpdate(
                WaitUntil.Completed,
                _dbName.Value,
                dataBaseData);

        return GetDBConnectionComponent();
    }

    public DBConnectionComponent Get(AzureCredentails credentials)
    {
        return GetDBConnectionComponent();;
    }

    private DBConnectionComponent GetDBConnectionComponent()
    {
        return new DBConnectionComponent($"""Server=tcp:{_serverName.Value}.database.windows.net,1433;Initial Catalog={_dbName.Value};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Default";""");
    }
}