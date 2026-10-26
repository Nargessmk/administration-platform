using System.IO.Compression;
using System.Reflection;
using System.Text;
using EntityFrameworkCore.UnitOfWork.Extensions;
using Sentry;
using Sentry.AspNetCore;
using Hangfire;
using MassTransit;
using Cloudland.APM.BackgroundTask.ActionFilters;
using Cloudland.APM.BackgroundTask.CQRS.Behaviours;
using Cloudland.APM.BackgroundTask.Extensions;
using Cloudland.APM.Mapper.Profiles;
using Cloudland.APM.Models.JWT;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Raven.DependencyInjection;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Cloudland.APM.DataBaseContext;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.APM.BackgroundTask.Services;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .Build();

var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
var secretKey = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey!);

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
    .SetApplicationName("MAYSA")
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddOpenApi();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new ConfigurationOptions
    {
        Ssl = true,
        Password = "oDYWsvEoguPrnHjpD1wFD0c7rexO7ZzaW9iwyQq2oXC6BZ1u2G4frMxObN9AxjKUSd3zlakXbyJPzXwz",
        ClientName = "SIN-PLATFORM",
        AbortOnConnectFail = true,
        SslHost = "redis.sin-platform.ir",
        EndPoints =
        {
            {
                "redis.sin-platform.ir", 6379
            }
        }
    };
    options.ConfigurationOptions.CertificateValidation += RedisExtensions.CheckServerCertificate!;
});

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalRCore();
builder.Services.AddRavenDbDocStore();
builder.Services.AddRavenDbAsyncSession();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<APMProfiles>();
});
builder.Services.AddDbContext<APMDBContext>();
builder.Services.AddScoped<Microsoft.EntityFrameworkCore.DbContext, APMDBContext>();
builder.Services.AddDbContext<SSSBDBContext>();
builder.Services.AddScoped<Microsoft.EntityFrameworkCore.DbContext, SSSBDBContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddElasticsearch(configuration);
builder.Services.AddScoped<IMediator, Mediator>();

builder.Services.AddUnitOfWork();

builder.Services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

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
builder.WebHost.UseSentry((SentryAspNetCoreOptions o) =>
{
    o.Dsn = builder.Configuration.GetValue<string>("Sentry:Dsn");
    o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddHealthChecks();
builder.Services.AddHangfire(c => c.UseSqlServerStorage(configuration["ConnectionStrings:SSSBConnectionString"]));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IReindexJobService, ReindexJobService>();

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<LogRequestResponseFilter>();
    })
    .AddNewtonsoftJson(x =>
        x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddHttpClient<IpApiClient>();
var app = builder.Build();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(opt => { opt.RouteTemplate = "openapi/{documentName}.json"; });
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "SEEN Platform Background Service API";
        opt.Theme = ScalarTheme.Mars;
        opt.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.UseCors();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/background-tasks-service-status", new HealthCheckOptions());
app.UseEndpoints(_ => { });

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ValidIssuer = jwtSettings.Issuer,
    ValidAudience = jwtSettings.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
};

var options = new DashboardOptions
{
    Authorization =
    [
        new HangfireDashboardJwtAuthorizationFilter(tokenValidationParameters, "admin")
    ],
    IgnoreAntiforgeryToken = true
};

app.MapOpenApi()
    .RequireAuthorization("ApiTesterPolicy")
    .CacheOutput();

app.MapControllers();


app.UseHangfireDashboard("/hangfire", options);
// app.UseHangfireDashboard("/hangfire");

// Schedule the reindex job to run every Friday at 4 PM
RecurringJob.AddOrUpdate<IReindexJobService>(
    "weekly-reindex-all",
    service => service.ExecuteReindexAllJobAsync(),
    "0 16 * * 5", // Cron expression for Friday at 4 PM (16:00)
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    });

var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:7007";
app.Run(urls);