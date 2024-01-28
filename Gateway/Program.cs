using Ocelot.Cache.Redis;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Get<string>());
});
builder.Configuration.AddJsonFile($"ocelot.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
//    .AddCacheManager(settings =>
//{
//    settings.WithDictionaryHandle();
//});
builder.Services.AddOcelotRedisIntegration(builder.Configuration.GetSection("Redis:ConnectionString").Get<string>());
var app = builder.Build();
app.UseSerilogRequestLogging();
app.MapGet("/", () => "Hello World!");
app.UseOcelot().GetAwaiter().GetResult();
app.Run();
