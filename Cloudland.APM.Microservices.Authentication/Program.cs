using System.IO.Compression;
using Asp.Versioning;
using Sentry;
using Sentry.AspNetCore;
using MassTransit;
using Cloudland.APM.Microservices.Authentication.ActionFilters;
using Cloudland.APM.Microservices.Authentication.Extensions;
using Cloudland.APM.Models;
using Cloudland.APM.Models.JWT;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
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
builder.Services.AddSignalRCore();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = new ConfigurationOptions
    {
        Ssl = false,
        ClientName = "APM-PLATFORM",
        AbortOnConnectFail = true,
        EndPoints =
        {
            {
                "localhost", 6379
            }
        }
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddJwtAuthentication(configuration);
builder.Services.RegisterServices(configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.WebHost.UseSentry((SentryAspNetCoreOptions o) =>
{
    o.Dsn = builder.Configuration.GetValue<string>("Sentry:Dsn");
    o.Debug = builder.Configuration.GetValue<bool>("Sentry:Debug");
});
builder.Services.AddOpenApi();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});
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
builder.Services.AddHttpClient<IpApiClient>();

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUploadSettings"));

var app = builder.Build();

// Ensure upload directory exists
/*
var uploadPath = builder.Configuration.GetSection("FileUploadSettings:UploadPath").Value;
if (!string.IsNullOrEmpty(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}
*/

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(opt => { opt.RouteTemplate = "openapi/{documentName}.json"; });
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "SEEN Platform Authenticate API";
        opt.Theme = ScalarTheme.Mars;
        opt.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.MapOpenApi()
    .RequireAuthorization("ApiTesterPolicy")
    .CacheOutput();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/api/health");
app.MapControllers();
var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:7001";
app.Run(urls);