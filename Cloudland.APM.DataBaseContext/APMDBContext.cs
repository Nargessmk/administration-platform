using Audit.Core;
using Audit.EntityFramework;
using Cloudland.APM.DatabaseConfiguration;
using Cloudland.APM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cloudland.APM.DataBaseContext;

public sealed class APMDBContext : AuditDbContext
{
    private readonly ILogger<APMDBContext>? _logger;
    private readonly Guid? CustomerId;
    public APMDBContext(DbContextOptions options) : base(options)
    {
        CustomerId ??= Guid.Empty;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public APMDBContext(DbContextOptions options, ILogger<APMDBContext> logger) : this(options)
    {
        _logger = logger;
    }

    public APMDBContext(
        DbContextOptions options,
        IHttpContextAccessor httpContextAccessor,
        IConfigurationRoot configuration,
        ILogger<APMDBContext> logger) : this(options, logger) // این سازنده، سازنده قبلی را صدا می‌زند که _logger را مقداردهی می‌کند.
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        string? customerIdHeader = httpContextAccessor.HttpContext?.Request.Headers["CustomerID"];
        if (Guid.TryParse(customerIdHeader, out Guid parsedGuid))
        {
            CustomerId = parsedGuid;
        }
        else
        {
            CustomerId = Guid.Empty; 
            _logger?.LogWarning("Customer ID header was missing or invalid: '{CustomerIdHeader}'.", customerIdHeader);
        }
    }

    public DbSet<Constant> Constants { get; set; }
    public DbSet<ConstantRecursive> ConstantRecursives { get; set; }
    public DbSet<UserConstantPermission> UserConstantPermissions { get; set; }
    public DbSet<Calendar> Calendars { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("SSSBConnectionString");
            optionsBuilder.UseSqlServer(connectionString);
        }

        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ConstantRecursiveConfiguration());
        modelBuilder.ApplyConfiguration(new ConstantConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarConfiguration());

        // مدیریت ایمن فراخوانی GetMethod برای GetConstantsRecursive
        var getConstantsRecursiveMethod = typeof(APMDBContext)
            .GetMethod(nameof(GetConstantsRecursive), [typeof(Guid)]);
        if (getConstantsRecursiveMethod == null)
        {
            throw new InvalidOperationException($"Method '{nameof(GetConstantsRecursive)}' not found on '{nameof(APMDBContext)}'. Ensure its signature matches.");
        }
        modelBuilder.HasDbFunction(getConstantsRecursiveMethod)
            .HasName("GetConstantsRecursive");

        // مدیریت ایمن فراخوانی GetMethod برای GetConstantsRecursiveWithPermissions
        var getConstantsRecursiveWithPermissionsMethod = typeof(APMDBContext)
            .GetMethod(
                nameof(GetConstantsRecursiveWithPermissions),
                [typeof(Guid), typeof(Guid?), typeof(string), typeof(string)]
            );
        if (getConstantsRecursiveWithPermissionsMethod == null)
        {
            throw new InvalidOperationException($"Method '{nameof(GetConstantsRecursiveWithPermissions)}' not found on '{nameof(APMDBContext)}'. Ensure its signature matches.");
        }
        modelBuilder.HasDbFunction(getConstantsRecursiveWithPermissionsMethod)
            .HasName("GetConstantsRecursiveWithPermissions");
    }

    public override void OnScopeCreated(IAuditScope auditScope)
    {
        Database.BeginTransaction();
    }

    public override void OnScopeSaving(IAuditScope auditScope)
    {
        try
        {
            Database.CurrentTransaction?.Commit();
        }
        catch (Exception ex)
        {
            // اضافه کردن بررسی null قبل از استفاده از _logger
            _logger?.LogError("Database Error :{Error}", ex);
            Database.CurrentTransaction?.Rollback();
            throw;
        }
    }

    public IQueryable<ConstantRecursive> GetConstantsRecursive(Guid ConstantRootId)
    {
        return FromExpression(() => GetConstantsRecursive(ConstantRootId));
    }

    public IQueryable<ConstantRecursive> GetConstantsRecursiveWithPermissions(Guid ConstantRootId, Guid? UserId,
        string? RoleId, string? GroupId)
    {
        return FromExpression(() => GetConstantsRecursiveWithPermissions(ConstantRootId, UserId, RoleId, GroupId));
    }
}
