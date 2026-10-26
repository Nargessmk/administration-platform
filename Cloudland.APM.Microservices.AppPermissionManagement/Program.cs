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
using Cloudland.APM.Microservices.AppPermissionManagement.ActionFilters;
using Cloudland.APM.Microservices.AppPermissionManagement.CQRS.Behaviours;
using Cloudland.APM.Microservices.AppPermissionManagement.Extensions;
using Cloudland.APM.Models;
using Cloudland.APM.Models.JWT;
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
        ClientName = "APM-PLATFORM",
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

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediator();
builder.Services.AddSignalRCore();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddDbContext<APMDBContext>();
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<APMProfiles>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddRavenDbDocStore();
builder.Services.AddRavenDbAsyncSession();

builder.Services.AddScoped<DbContext, APMDBContext>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddTransient<IFusionAuthAsyncClient>(_ =>
    new FusionAuthClient(configuration.GetSection("FusionAuth")["APIKEY"],
        configuration.GetSection("FusionAuth")["AuthURL"]));

builder.Services.AddUnitOfWork();

builder.Services.AddRepository<IRepository<Constant>, Repository<Constant>>();
builder.Services.AddRepository<IRepository<ConstantRecursive>, Repository<ConstantRecursive>>();
builder.Services.AddRepository<IRepository<UserConstantPermission>, Repository<UserConstantPermission>>();

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
builder.WebHost.UseSentry((SentryAspNetCoreOptions o) =>
{
    o.Dsn = builder.Configuration.GetValue<string>("Sentry:Dsn");
    o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("CoreConnectionString") ?? string.Empty, name: "DatabaseHealthCheck");

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
        opt.Title = "APM Platform Course Management Service API";
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
var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:7003";
app.Run(urls);