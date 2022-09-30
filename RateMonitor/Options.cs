using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateMonitor
{
    public class LimiterOptions
    {
        public const string Section = "Limiter";

        public int Limit { get; set; } = Int32.MaxValue;

        public double NotifyOnPercentage { get; set; } = 50;
    }

    public class RedisOptions
    {
        public const string Section = "Redis";

        public string Host { get; set; } = string.Empty;
    }

    public class MessageBusOptions
    {
        public const string Section = "Topics";

        public string ApiRateHitTopic { get; set; } = string.Empty;

        public string NotificationServiceTopic { get; set; } = string.Empty;
    }
}
