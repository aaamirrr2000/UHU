using DbUp;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

using System;
using System.Reflection;

namespace NG.MicroERP.API.Services;

public class DatabaseMigrator(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("Default")!;

    public void Migrate()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            Console.WriteLine("Error: Connection string is missing.");
            return;
        }

        MySqlConnectionStringBuilder builder = new(_connectionString);
        string server = builder.Server;
        string userId = builder.UserID;
        string password = builder.Password;
        string databaseName = builder.Database;

        if (string.IsNullOrEmpty(databaseName))
        {
            Console.WriteLine("Error: Database name is missing in the connection string.");
            return;
        }

        string connectionStringWithoutDatabase = $"Server={server};Uid={userId};Pwd={password};";

        using (MySqlConnection connection = new(connectionStringWithoutDatabase))
        {
            connection.Open();
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = $@"
                CREATE DATABASE IF NOT EXISTS `{databaseName}`;
                USE `{databaseName}`;";

            _ = cmd.ExecuteNonQuery();
        }

        string fullConnectionString = $"Server={server};Database={databaseName};Uid={userId};Pwd={password};";

        string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        Console.WriteLine("Embedded resources in assembly:");
        foreach (string resourceName in resourceNames)
        {
            Console.WriteLine(resourceName);
        }

        DbUp.Engine.UpgradeEngine upgrader = DeployChanges.To
            .MySqlDatabase(fullConnectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.Contains(".Scripts."))
            .LogToConsole()
            .Build();

        DbUp.Engine.DatabaseUpgradeResult result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.WriteLine("Migration failed!");
            Console.WriteLine(result.Error);
            //throw new Exception("Database migration failed.", result.Error);
        }

        Console.WriteLine("Database migration successful.");
    }
}