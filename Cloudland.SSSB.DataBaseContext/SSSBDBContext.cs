using Audit.Core;
using Audit.EntityFramework;
using Cloudland.APM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.DatabaseConfiguration;
using Cloudland.SSSB.DatabaseConfiguration.Views;
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.DataBaseContext.Extensions;
using ReservationRequestLetterViewConfiguration = Cloudland.SSSB.DatabaseConfiguration.Views.ReservationRequestLetterViewConfiguration;

namespace Cloudland.SSSB.DataBaseContext;

public sealed class SSSBDBContext : AuditDbContext
{
    private readonly ILogger<SSSBDBContext>? _logger;
    private readonly Guid? CustomerId;

    public SSSBDBContext(DbContextOptions options) : base(options)
    {
        CustomerId ??= Guid.Empty;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public SSSBDBContext(DbContextOptions options, ILogger<SSSBDBContext> logger) : this(options)
    {
        _logger = logger;
    }

    public SSSBDBContext(
        DbContextOptions options,
        IHttpContextAccessor httpContextAccessor,
        IConfigurationRoot configuration,
        ILogger<SSSBDBContext> logger) :
        this(options, logger) // این سازنده، سازنده قبلی را صدا می‌زند که _logger را مقداردهی می‌کند.
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

    public DbSet<OrganizationalChart> OrganizationalCharts { get; set; }
    public DbSet<AccountNumber> AccountNumbers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Contractor> Contractors { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<Food> Foods { get; set; }
    //public DbSet<Calendar> Calendars { get; set; }
    public DbSet<ContractFoodCalendar> ContractFoodCalendars { get; set; }
    public DbSet<WeekDay> WeekDays { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Personnel> Personnels { get; set; }
    public DbSet<VehicleDriver> VehicleDrivers { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Bed> Beds { get; set; }
    public DbSet<OrganizationalPosition>  OrganizationalPositions { get; set; }
    public DbSet<Contract>  Contracts { get; set; }
    public DbSet<ContractLocation>  ContractLocations { get; set; }
    public DbSet<ContractFood>  ContractFoods { get; set; }
    public DbSet<ContractFoodLocation>  ContractFoodLocations { get; set; }
    public DbSet<ReservationRequestHeader>  ReservationRequestHeaders { get; set; }
    public DbSet<ReservationRequestLetter>  ReservationRequestLetters { get; set; }
    public DbSet<ReservationFoodCalendar>  ReservationFoodCalendars { get; set; }
    public DbSet<ReservationRequestBed> ReservationRequestBeds { get; set; }
    public DbSet<ReservationRequestPerson>  ReservationRequestPersons { get; set; }
    
    public DbSet<ContractRecursive>  ContractRecursives { get; set; }
    
    // View DbSets
    public DbSet<LocationView> LocationViews { get; set; }
    public DbSet<OrganizationalChartView> OrganizationalChartViews { get; set; }
    public DbSet<OrganizationalPositionView> OrganizationalPositionViews { get; set; }
    public DbSet<PersonnelView> PersonnelViews { get; set; }
    public DbSet<VehicleView> VehicleViews { get; set; }
    public DbSet<MealView> MealViews { get; set; }
    public DbSet<FoodView> FoodViews { get; set; }
    public DbSet<ContractorView> ContractorViews { get; set; }
    public DbSet<ContractView> ContractViews { get; set; }
    public DbSet<ContractLocationView> ContractLocationViews { get; set; }
    public DbSet<ContractFoodView> ContractFoodViews { get; set; }
    public DbSet<ContractFoodLocationView> ContractFoodLocationViews { get; set; }
    public DbSet<ContractFoodCalendarView> MealCalendarViews { get; set; }
    public DbSet<WeekDayView> WeekDayViews { get; set; }
    public DbSet<ReservationRequestHeaderView> ReservationRequestHeaderViews { get; set; }
    public DbSet<ReservationRequestPersonView> ReservationRequestPersonViews { get; set; }
    public DbSet<ReservationRequestBedView> ReservationRequestBedViews { get; set; }
    public DbSet<ReservationRequestLetterView> ReservationRequestLetterViews { get; set; }
    public DbSet<RoomView> RoomViews { get; set; }
    public DbSet<BedView> BedViews { get; set; }
    public DbSet<VehicleDriverView> VehicleDriverViews { get; set; }
    public DbSet<AccountNumberView> AccountNumberViews { get; set; }
    public DbSet<ReservationFoodCalendarView> ReservationFoodCalendarViews { get; set; }
    
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ConstantConfiguration());
        modelBuilder.ApplyConfiguration(new CalendarConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationalChartConfiguration());
        modelBuilder.ApplyConfiguration(new AccountNumberConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new ContractorConfiguration());
        modelBuilder.ApplyConfiguration(new MealConfiguration());
        modelBuilder.ApplyConfiguration(new FoodConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodCalendarConfiguration());
        modelBuilder.ApplyConfiguration(new PersonnelConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleDriverConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new BedConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationalPositionConfiguration());
        modelBuilder.ApplyConfiguration(new ContractConfiguration());
        modelBuilder.ApplyConfiguration(new ContractLocationConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodLocationConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new DatabaseConfiguration.ReservationRequestLetterConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationFoodCalendarConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestBedConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestPersonConfiguration());
        modelBuilder.ApplyConfiguration(new WeekDayConfiguration());
        
        //Views
        modelBuilder.ApplyConfiguration(new OrganizationalChartViewConfiguration());
        modelBuilder.ApplyConfiguration(new LocationViewConfiguration());
        modelBuilder.ApplyConfiguration(new PersonnelViewConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleViewConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationalPositionViewConfiguration());
        modelBuilder.ApplyConfiguration(new FoodViewConfiguration());
        modelBuilder.ApplyConfiguration(new MealViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractorViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractLocationViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodLocationViewConfiguration());
        modelBuilder.ApplyConfiguration(new ContractFoodCalendarViewConfiguration());
        modelBuilder.ApplyConfiguration(new WeekDayViewConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestHeaderViewConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestPersonViewConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestBedViewConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationRequestLetterViewConfiguration());
        modelBuilder.ApplyConfiguration(new RoomViewConfiguration());
        modelBuilder.ApplyConfiguration(new BedViewConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleDriverViewConfiguration());
        modelBuilder.ApplyConfiguration(new AccountNumberViewConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationFoodCalendarViewConfiguration());
        
        // Configure database functions
        modelBuilder.ConfigureDatabaseFunctions();
    }
    
    public IQueryable<ConstantRecursive> GetOrganizationalChartRecursive(Guid rootId)
    {
        return FromExpression(() => GetOrganizationalChartRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetLocationRecursive(Guid rootId)
    {
        return FromExpression(() => GetLocationRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetPersonnelRecursive(Guid rootId)
    {
        return FromExpression(() => GetPersonnelRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetVehicleRecursive(Guid rootId)
    {
        return FromExpression(() => GetVehicleRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetOrganizationalPositionRecursive(Guid rootId)
    {
        return FromExpression(() => GetOrganizationalPositionRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetFoodRecursive(Guid rootId)
    {
        return FromExpression(() => GetFoodRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetMealRecursive(Guid rootId)
    {
        return FromExpression(() => GetMealRecursive(rootId));
    }

    public IQueryable<ConstantRecursive> GetCalendarRecursive(Guid rootId)
    {
        return FromExpression(() => GetCalendarRecursive(rootId));
    }
    
    public IQueryable<ConstantRecursive> GetWeekDayRecursive(Guid rootId)
    {
        return FromExpression(() => GetWeekDayRecursive(rootId));
    }

    public IQueryable<ConstantRecursive> GetContractorRecursive(Guid rootId)
    {
        return FromExpression(() => GetContractorRecursive(rootId));
    }
    public IQueryable<ContractRecursive> GetContractRecursive()
    {
        return FromExpression(() => GetContractRecursive());
    }
}