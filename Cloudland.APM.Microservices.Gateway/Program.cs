using System.IO.Compression;
using Asp.Versioning;
using Sentry;
using Sentry.AspNetCore;
using Cloudland.APM.Microservices.Gateway.ActionFilters;
using Cloudland.APM.Microservices.Gateway.Extensions;
using Cloudland.APM.Models.JWT;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile("ocelot.json", false, true)
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


builder.Services.AddMemoryCache();

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
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("version"),
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new MediaTypeApiVersionReader("version")
    );
});
builder.Services.AddJwtAuthentication(configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot(configuration).AddPolly();
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


builder.Services.Configure<KestrelServerOptions>(options => { options.Limits.MaxRequestBodySize = int.MaxValue; });

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddHttpClient<IpApiClient>();
builder.Services.AddHttpClient();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogRequestResponseFilter>();
});


var app = builder.Build();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(opt => { opt.RouteTemplate = "openapi/{documentName}.json"; });
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "SEEN Platform Gateway API";
        opt.Theme = ScalarTheme.Mars;
        opt.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.UseCors();
app.UseWebSockets();

app.UseRouting();
app.MapHealthChecks("/gateway-service-status", new HealthCheckOptions());
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RateLimitingMiddleware>();
app.MapOpenApi()
    .RequireAuthorization("ApiTesterPolicy")
    .CacheOutput();
app.MapControllers();
await app.UseOcelot();
var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://localhost:7000";
await app.RunAsync(urls);
