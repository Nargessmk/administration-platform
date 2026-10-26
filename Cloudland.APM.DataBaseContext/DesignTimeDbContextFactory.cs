using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.DataBaseContext;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<APMDBContext>
{
    public APMDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<APMDBContext>();
        var connectionString = configuration.GetConnectionString("SSSBConnectionString");
        builder.UseSqlServer(connectionString);
        return new APMDBContext(builder.Options);
    }
}