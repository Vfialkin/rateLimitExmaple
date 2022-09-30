using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace RateLimitingMiddleware.Dispatcher;

public class RedisDispatcher<T> : IDispatcher<T>
{
    private readonly ConnectionMultiplexer _multiplexer;
    private readonly MessageBusOptions _options;
    private readonly ILogger _logger;

    public RedisDispatcher(ConnectionMultiplexer multiplexer, IOptions<MessageBusOptions> options, ILogger<RedisDispatcher<T>> logger)
    {
        _multiplexer = multiplexer;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Dispatch(T apiPath, CancellationToken cts)
    {
        //cutting corners :)
        var serializedMessage = apiPath.ToString();

        await _multiplexer.GetSubscriber().PublishAsync(_options.ApiRateHitTopic, serializedMessage, CommandFlags.FireAndForget);
        
        _logger.LogInformation($"Dispatched {apiPath}");
    }
}