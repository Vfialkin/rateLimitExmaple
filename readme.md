# Example of using Redis as a pub sub and a db for rate limit calculation.

In this example we dont need to block API, just notify if API Hit Limit was exceeded so everything is processed async.
- Counting distributed nature of Redis we could do the same without a seaprate service and a bus but current implemention allows to change behavior of rate limiter without update of middleware in each service.
- Solution has some obvious flaws not covered as its a demo code. 
- Some extra optimizations like batching or processing in paralell where skipped for simplicity

![rateLimitExmaple](rateLimitExmaple.png)

## Web API
- Example of a web app on minimal web-api for testing middlware with Swagger UI

## Middleware 
- Publishes endpoint Name to in-memory queue on any API hit
- Background job relays those messages into a message bus (Redis in this case)
- If external dependency (redis) is not available we are not blocking API call and still have a chance to publish the message on retry
- Threading.Channels support concurrent pub\sub so it can be easily scaled out by setting more publishers 

## API Hit Rate Monitoring Service
- Reads messages from a bus (redis)
- Calculates current API hit ratio using redis `String` with 30 days expiration and `Incr` function (https://redis.io/commands/incr/)
- Dispatches notification if we reached the limit and makrs counter with `notified` flag

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

### Dependency and testing
#### Redis: 
```
docker run --name some-redis -p 6379:6379 -d redis
```
Swagger UI will open automaticly on startup.
