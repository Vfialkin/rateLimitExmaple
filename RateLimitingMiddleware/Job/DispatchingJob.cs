using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RateLimitingMiddleware.Dispatcher;

namespace RateLimitingMiddleware.Job;

/// <summary>
/// Background job
/// Read all from in-memory queue and publish to external queue for actual processing
/// </summary>
/// <typeparam name="T"></typeparam>
public class DispatchingJob<T> : BackgroundService
{
    private readonly Channel<T> _channel;
    private readonly IDispatcher<T> _dispatcher;
    private readonly ILogger<DispatchingJob<T>> _logger;

    public DispatchingJob(Channel<T> channel, IDispatcher<T> dispatcher, ILogger<DispatchingJob<T>> logger)
    {
        _channel = channel;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cts)
    {
        _logger.LogInformation($"{nameof(DispatchingJob<T>)} started");
        while (!cts.IsCancellationRequested)
        {
            try
            {
                await foreach (var message in _channel.Reader.ReadAllAsync(cts))
                {
                    //todo: some batching and grouping could be beneficial here for performance
                    await _dispatcher.Dispatch(message, cts);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"{nameof(DispatchingJob<T>)} was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                //carry on, dont kill the job, we want it retry
                _logger.LogError(ex, "Error");
                await Task.Delay(100, cts);
            }
        }
    }
}