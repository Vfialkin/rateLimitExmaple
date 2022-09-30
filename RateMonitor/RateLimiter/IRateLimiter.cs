namespace RateMonitor.RateLimiter;

public interface IRateLimiter
{
    Task CalculateLimit(string apiPath, Task notificationTask);
}