using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cloudland.SSSB.DataBaseContext;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SSSBDBContext>
{
    public SSSBDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<SSSBDBContext>();
        var connectionString = configuration.GetConnectionString("SSSBConnectionString");
        builder.UseSqlServer(connectionString);
        return new SSSBDBContext(builder.Options);
    }
}