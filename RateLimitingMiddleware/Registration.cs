using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RateLimitingMiddleware.Dispatcher;
using RateLimitingMiddleware.Job;
using StackExchange.Redis;

namespace RateLimitingMiddleware;

public static class Registration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>().Bind(configuration.GetSection(RedisOptions.Section));
        services.AddOptions<MessageBusOptions>().Bind(configuration.GetSection(MessageBusOptions.Section));

        var redisOptions = configuration
            .GetSection(RedisOptions.Section)
            .Get<RedisOptions>();

        services
            .AddTransient<IDispatcher<string>, RedisDispatcher<string>>()
            .AddSingleton(_ => Channel.CreateUnbounded<string>())
            .AddSingleton(_ => ConnectionMultiplexer.Connect(redisOptions.Host));

        services.AddHostedService<DispatchingJob<string>>();

        return services;
    }

    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(builder.ApplicationServices.GetRequiredService<Channel<string>>());
    }
}