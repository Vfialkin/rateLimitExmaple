using Microsoft.Extensions.Options;
using RateMonitor;
using RateMonitor.RateLimiter;
using StackExchange.Redis;

namespace RateMonitoringService.Job
{
    public class RedisRateConsumer<T> : BackgroundService
    {
        private readonly ILogger<RedisRateConsumer<T>> _logger;
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly IRateLimiter _rateLimiter;
        private readonly MessageBusOptions _busOptions;

        public RedisRateConsumer(ConnectionMultiplexer multiplexer, IRateLimiter rateLimiter, IOptions<MessageBusOptions> busOptions,  ILogger<RedisRateConsumer<T>> logger)
        {
            _multiplexer = multiplexer;
            _rateLimiter = rateLimiter;
            _busOptions = busOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cts)
        {
            _logger.LogInformation($"{nameof(RedisRateConsumer<T>)} is starting.");
            cts.Register(() => _logger.LogInformation($"{nameof(RedisRateConsumer<T>)} was canceled."));

            await _multiplexer.GetSubscriber().SubscribeAsync(_busOptions.ApiRateHitTopic,async (_, apiPath) =>
            {
                await _rateLimiter.CalculateLimit(apiPath, NotificationTask(apiPath));
            });

            await Task.Delay(Timeout.Infinite, cts);
            _logger.LogInformation($"{nameof(RedisRateConsumer<T>)} is stopping.");

            //inform emailing service about the limit reach
            Task<long> NotificationTask(RedisValue apiPath)
            {
                return _multiplexer.GetSubscriber().PublishAsync(_busOptions.NotificationServiceTopic, apiPath);
            }

        }
    }
}
