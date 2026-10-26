using System.IO.Compression;
using System.Reflection;
using Asp.Versioning;
using EntityFrameworkCore.Repository;
using EntityFrameworkCore.Repository.Extensions;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Extensions;
using Sentry;
using Sentry.AspNetCore;
using HealthChecks.UI.Client;
using io.fusionauth;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.Mapper.Profiles;
using Cloudland.APM.Models;
using Cloudland.APM.Models.JWT;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.Mapper.Profiles;
using Cloudland.SSSB.Models;
using Cloudland.SSSB.DTO.Validation;
using Cloudland.SSSB.Microservices.AppManagement.ActionFilters;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Behaviours;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Raven.DependencyInjection;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .Build();


var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod();
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin();
        }
    });
});

builder.AddSerilog();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"DataProtectionKeys/"))
    .SetApplicationName("APM")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14));
builder.Services.AddResponseCaching();
builder.Services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
builder.Services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
builder.Services.AddResponseCompression(action =>
{
    action.Providers.Add<BrotliCompressionProvider>();
    action.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            []
        }
    });
});

builder.Services.AddJwtAuthentication(configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new ConfigurationOptions
    {
        ClientName = "SSSB-PLATFORM",
        AbortOnConnectFail = true,
        EndPoints =
        {
            {
                "localhost", 6379
            }
        }
    };
    //options.ConfigurationOptions.CertificateValidation += RedisExtensions.CheckServerCertificate!;
});
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<APMProfiles>();
    config.AddProfile<SSSBProfiles>();
});
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediator();
builder.Services.AddSignalRCore();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddDbContext<APMDBContext>();
builder.Services.AddDbContext<SSSBDBContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRavenDbDocStore();
builder.Services.AddRavenDbAsyncSession();

builder.Services.AddScoped<DbContext, APMDBContext>();
builder.Services.AddScoped<DbContext, SSSBDBContext>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddTransient<IFusionAuthAsyncClient>(_ =>
    new FusionAuthClient(configuration.GetSection("FusionAuth")["APIKEY"],
        configuration.GetSection("FusionAuth")["AuthURL"]));

builder.Services.AddUnitOfWork();


#region Model Repos
builder.Services.AddRepository<IRepository<Constant>, Repository<Constant>>();
builder.Services.AddRepository<IRepository<ConstantRecursive>, Repository<ConstantRecursive>>();
builder.Services.AddRepository<IRepository<UserConstantPermission>, Repository<UserConstantPermission>>();
builder.Services.AddRepository<IRepository<Calendar>, Repository<Calendar>>();
builder.Services.AddRepository<IRepository<OrganizationalChart>, Repository<OrganizationalChart>>();
builder.Services.AddRepository<IRepository<Vehicle>, Repository<Vehicle>>();
builder.Services.AddRepository<IRepository<VehicleDriver>, Repository<VehicleDriver>>();
builder.Services.AddRepository<IRepository<Personnel>, Repository<Personnel>>();
builder.Services.AddRepository<IRepository<Food>, Repository<Food>>();
builder.Services.AddRepository<IRepository<Meal>, Repository<Meal>>();
builder.Services.AddRepository<IRepository<Room>, Repository<Room>>();
builder.Services.AddRepository<IRepository<Bed>, Repository<Bed>>();
builder.Services.AddRepository<IRepository<OrganizationalPosition>, Repository<OrganizationalPosition>>();
builder.Services.AddRepository<IRepository<Contract>, Repository<Contract>>();
builder.Services.AddRepository<IRepository<ContractRecursive>, Repository<ContractRecursive>>();
builder.Services.AddRepository<IRepository<ContractLocation>, Repository<ContractLocation>>();
builder.Services.AddRepository<IRepository<ContractFood>, Repository<ContractFood>>();
builder.Services.AddRepository<IRepository<Location>, Repository<Location>>();
builder.Services.AddRepository<IRepository<WeekDay>, Repository<WeekDay>>();
builder.Services.AddRepository<IRepository<Contractor>, Repository<Contractor>>();
builder.Services.AddRepository<IRepository<AccountNumber>, Repository<AccountNumber>>();
builder.Services.AddRepository<IRepository<ContractFoodLocation>, Repository<ContractFoodLocation>>();
builder.Services.AddRepository<IRepository<ContractFoodCalendar>, Repository<ContractFoodCalendar>>();
builder.Services.AddRepository<IRepository<ReservationRequestHeader>, Repository<ReservationRequestHeader>>();
builder.Services.AddRepository<IRepository<ReservationRequestLetter>, Repository<ReservationRequestLetter>>();
builder.Services.AddRepository<IRepository<ReservationFoodCalendar>, Repository<ReservationFoodCalendar>>();
builder.Services.AddRepository<IRepository<ReservationRequestBed>, Repository<ReservationRequestBed>>();
builder.Services.AddRepository<IRepository<ReservationRequestPerson>, Repository<ReservationRequestPerson>>();
#endregion

#region View Repos
builder.Services.AddRepository<IRepository<LocationView>, Repository<LocationView>>();
builder.Services.AddRepository<IRepository<OrganizationalChartView>, Repository<OrganizationalChartView>>();
builder.Services.AddRepository<IRepository<OrganizationalPositionView>, Repository<OrganizationalPositionView>>();
builder.Services.AddRepository<IRepository<PersonnelView>, Repository<PersonnelView>>();
builder.Services.AddRepository<IRepository<VehicleView>, Repository<VehicleView>>();
builder.Services.AddRepository<IRepository<MealView>, Repository<MealView>>();
builder.Services.AddRepository<IRepository<FoodView>, Repository<FoodView>>();
builder.Services.AddRepository<IRepository<ContractorView>, Repository<ContractorView>>();
builder.Services.AddRepository<IRepository<ContractView>, Repository<ContractView>>();
builder.Services.AddRepository<IRepository<ContractLocationView>, Repository<ContractLocationView>>();
builder.Services.AddRepository<IRepository<ContractFoodView>, Repository<ContractFoodView>>();
builder.Services.AddRepository<IRepository<ContractFoodLocationView>, Repository<ContractFoodLocationView>>();
builder.Services.AddRepository<IRepository<WeekDayView>, Repository<WeekDayView>>();
builder.Services.AddRepository<IRepository<ContractFoodCalendarView>, Repository<ContractFoodCalendarView>>();
builder.Services.AddRepository<IRepository<ReservationRequestHeaderView>, Repository<ReservationRequestHeaderView>>();
builder.Services.AddRepository<IRepository<ReservationRequestPersonView>, Repository<ReservationRequestPersonView>>();
builder.Services.AddRepository<IRepository<ReservationRequestBedView>, Repository<ReservationRequestBedView>>();
builder.Services.AddRepository<IRepository<ReservationRequestLetterView>, Repository<ReservationRequestLetterView>>();
builder.Services.AddRepository<IRepository<RoomView>, Repository<RoomView>>();
builder.Services.AddRepository<IRepository<BedView>, Repository<BedView>>();
builder.Services.AddRepository<IRepository<VehicleDriverView>, Repository<VehicleDriverView>>();
builder.Services.AddRepository<IRepository<AccountNumberView>, Repository<AccountNumberView>>();
builder.Services.AddRepository<IRepository<ReservationFoodCalendarView>, Repository<ReservationFoodCalendarView>>();
#endregion

builder.Services.AddSSSBValidators();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));


builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification.service", false));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddElasticsearch(configuration);
builder.Services.AddScoped<Cloudland.SSSB.Microservices.AppManagement.Services.IReindexService, Cloudland.SSSB.Microservices.AppManagement.Services.ReindexService>();
builder.WebHost.UseSentry((SentryAspNetCoreOptions o) =>
{
    o.Dsn = builder.Configuration.GetValue<string>("Sentry:Dsn");
    o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("SSSBConnectionString") ?? string.Empty, name: "DatabaseHealthCheck");

builder.Services.AddOpenApi();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Default version: 1.0
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("version"),
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new MediaTypeApiVersionReader("version")
    );
});


builder.Services.AddControllers(options =>
    {
        options.Filters.Add<LogRequestResponseFilter>();
    })
    .AddOData()
    .AddNewtonsoftJson(x =>
        x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();
app.UseForwardedHeaders();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(opt => { opt.RouteTemplate = "openapi/{documentName}.json"; });
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "SSSB Platform Course Management Service API";
        opt.Theme = ScalarTheme.Mars;
        opt.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.UseCors();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapOpenApi()
    .RequireAuthorization("ApiTesterPolicy")
    .CacheOutput();
app.MapControllers();

var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:7004";
app.Run(urls);