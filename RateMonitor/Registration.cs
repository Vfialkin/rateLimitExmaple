using Microsoft.Extensions.Configuration;
using RateMonitor.RateLimiter;
using RateMonitoringService.Job;
using RateMonitoringService.RateLimiter;
using StackExchange.Redis;

namespace RateMonitor;

public static class Registration
{
    public static IServiceCollection RegisterJob(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LimiterOptions>().Bind(configuration.GetSection(LimiterOptions.Section));
        services.AddOptions<RedisOptions>().Bind(configuration.GetSection(RedisOptions.Section));
        services.AddOptions<MessageBusOptions>().Bind(configuration.GetSection(MessageBusOptions.Section));
       
        return services
            .AddHostedService<RedisRateConsumer<string>>()
            .AddTransient<IRateLimiter, RedisRateLimiter>()
            .AddSingleton(_ => ConnectionMultiplexer.Connect("localhost:6379"));
    }

}