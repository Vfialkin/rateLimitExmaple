using RateLimitingMiddleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddLogging()
    .AddRateLimiting(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger()
        .UseSwaggerUI();

    app.Urls.Add("https://localhost:5221");
}

//some APIs to test limiter
app.MapGet("/", () => "Nothing here");
app.MapGet("/HelloWorld", () => "Hello World!");
app.MapGet("/CurrentTime", () => DateTime.Now);

//register middleware
app.UseRateLimiting();

app.Run();