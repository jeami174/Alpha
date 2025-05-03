using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace Data.Context;

/// <summary>
/// Design-time factory for creating <see cref="DataContext"/> instances,
/// used by Entity Framework Core tools to apply migrations and scaffold the database.
/// Reads the connection string from appsettings.json and configures SQL Server.
/// </summary>
public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    /// <summary>
    /// Creates a new <see cref="DataContext"/> instance configured with the
    /// 'DefaultConnection' SQL Server connection string from appsettings.json.
    /// </summary>
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        { 
            Console.Error.WriteLine("Connection string 'DefaultConnection' hittades inte. Kontrollera att appsettings.json finns på rätt sökväg.");
            throw new InvalidOperationException("Connection string 'DefaultConnection' hittades inte.");
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
