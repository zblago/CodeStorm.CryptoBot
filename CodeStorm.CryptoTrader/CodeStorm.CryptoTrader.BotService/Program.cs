using CodeStorm.CryptoTrader.Application.ApplicationServices;
using CodeStorm.CryptoTrader.BotService;
using CodeStorm.CryptoTrader.Repository.DatabaseContext;
using CodeStorm.CryptoTrader.Repository.Repository;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<ResponseRepository>();
builder.Services.AddScoped<TimelineAnalysisRepository>();
builder.Services.AddScoped<ActionSignalRepository>();
builder.Services.AddScoped<ExecutedActionRepository>();
builder.Services.AddScoped<RealTimeIndicatorService>();
builder.Services.AddScoped<CryptoTraderDbContext>();
builder.Services.AddHttpClient();

builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<CryptoTraderDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

var host = builder.Build();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

host.Run();
