namespace RateLimitingMiddleware.Dispatcher;

public interface IDispatcher<in T>
{
    Task Dispatch(T apiPath, CancellationToken cts);
}