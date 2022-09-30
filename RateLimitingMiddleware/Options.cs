public class RedisOptions
{
    public const string Section = "Redis";

    public string Host { get; set; } = string.Empty;
}

public class MessageBusOptions
{
    public const string Section = "Topics";

    public string ApiRateHitTopic { get; set; } = string.Empty;
}