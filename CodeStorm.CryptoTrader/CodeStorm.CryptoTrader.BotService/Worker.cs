using CodeStorm.CryptoTrader.Application;
using CodeStorm.CryptoTrader.Application.ApplicationServices;

namespace CodeStorm.CryptoTrader.BotService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var realTimeIndicatorService = scope.ServiceProvider.GetRequiredService<RealTimeIndicatorService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    }

                    await realTimeIndicatorService.GetLatestOHLCForFwog();

                    await Task.Delay(10000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the worker.");
                    // Optionally, you can add a delay here to avoid rapid retries in case of persistent errors.
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}
