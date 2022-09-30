# Example of using Redis as a pub sub and a db for rate limit calculation.

In this example we dont need to block API, just notify if Limit was exceeded so everything is processed async.
- Solution has some obvious flaws not covered as its a demo code. 

![rateLimitExmaple](rateLimitExmaple.png.png)

## Web API
- Example for testing with Swagger UI

## Middleware 
- Publishes endpoint Name to in-memory queue on any API hit
- Background job relays those messages into a message bus (Redis in this case but )
- If external dependency (redis) is not available we are not blocking API call and still have a chance to publish the message on retry

## API Hit Rate Monitoring Service
- Reads messages from a bus (redis)
- Calculates current API hit ratio using redis `Incr` function (https://redis.io/commands/incr/)
- Dispatches notification if we reached the limit

Rate limit and other values are stored in apssetings.json
```
  "Limiter": {
    "Limit": 4,
    "NotifyOnPercentage": 50 
  },
  "Redis": {
    "Host": "localhost"
  },
  "Topics": {
    "ApiRateHitTopic": "apiRateHitTopic",
    "NotificationServiceTopic": "notificationTopic"
  }
  ```

## Emailing service 
- Not part of this example

### Dependency:
Redis: 
```
docker run --name some-redis -p 6379:6379 -d redis
```
