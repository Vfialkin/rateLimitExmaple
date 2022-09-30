using Microsoft.Extensions.Options;
using RateMonitor;
using RateMonitor.RateLimiter;
using StackExchange.Redis;

namespace RateMonitoringService.RateLimiter;

/// <summary>
/// Redis based implementation
/// Creates one key for each api with one month expiration date
/// Increments value until it hits the limit and then changes Flag into value to stop tracking
/// Implementation has some flaws, we might fail to notify after setting the limit but it can be handled by moving logic into redis lua scripts
/// </summary>
public class RedisRateLimiter : IRateLimiter
{
    private readonly ILogger _logger;
    private readonly ConnectionMultiplexer _multiplexer;
    private readonly LimiterOptions _limiterOptions;

    const int DaysInMonth = 30;
    const string NotifiedValueFlag = "notified";

    public RedisRateLimiter(ConnectionMultiplexer multiplexer, IOptions<LimiterOptions> limiterOptions, ILogger<RedisRateLimiter> logger)
    {
        _multiplexer = multiplexer;
        _limiterOptions = limiterOptions.Value;
        _logger = logger;
    }

    public async Task CalculateLimit(string apiPath, Task notificationTask)
    {
        var db = _multiplexer.GetDatabase();
        var counter = await db.StringGetAsync(apiPath);

        if (counter == NotifiedValueFlag)
        {
            _logger.LogDebug($"Already notified for {apiPath}. Skipping");
            return;
        }

        if (!counter.IsNullOrEmpty && NotificationLimitExceded(counter))
        {
            _logger.LogWarning($"Rate limit For API {apiPath} exceeded limit");

            /*set Flag to stop incrementing value*/
            var prevValue = await db.StringGetSetAsync(apiPath, NotifiedValueFlag);

            /*If we are the fist to set the flag then we should notify Emailing*/
            if (prevValue != NotifiedValueFlag)
            {
                await notificationTask;
                _logger.LogDebug($"Notified about rate limit for {apiPath}");
            }
        }
        else
        {
            var currentCounterValue = await db.StringIncrementAsync(apiPath);
            //in prod scenario can be done by executing a lua script to make transactional with increment call
            if (currentCounterValue == 1)
            {
                _logger.LogDebug($"If it was a first call for API {apiPath}, setting expiration to {DaysInMonth} days");
                await db.KeyExpireAsync(apiPath, TimeSpan.FromDays(DaysInMonth), ExpireWhen.HasNoExpiry);
            }
        }

        bool NotificationLimitExceded(RedisValue redisValue)
        {
            var percentageNow = ((double)redisValue / _limiterOptions.Limit) * 100;
            return percentageNow >= _limiterOptions.NotifyOnPercentage;
        }

    }
}