using AutoMapper;
using HNBS.Config;
using HNBS.Services;
using HNBS.Services.IServices;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Auto mapper related
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();
builder.Services.AddHttpClient("HNBS", u => u.BaseAddress = new Uri(builder.Configuration["ServiceURLs:HNBSAPI"]));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Get<string>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("api/hnbs", async (int numberOfStories, IHackerNewsService _hackerNewsService) =>
{


    return await _hackerNewsService.GetHackerNewsBestStories(numberOfStories);

}).WithName("GetHackerNewsBestStories").WithOpenApi();

app.Run();