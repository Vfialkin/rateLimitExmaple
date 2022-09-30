using Microsoft.AspNetCore.Http;
using System.Threading.Channels;

namespace RateLimitingMiddleware
{ 
    /// <summary>
    /// None blocking tracking of API hits
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly Channel<string> _channel;
        private readonly RequestDelegate _next;
        
        public RateLimitingMiddleware(RequestDelegate next, Channel<string> channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var apiDisplayName= context.GetEndpoint()?.DisplayName;

            //if an actual endpoint
            if (!string.IsNullOrWhiteSpace(apiDisplayName))
            {
                //push for processing 
                await _channel.Writer.WriteAsync(apiDisplayName);
            }

            //and carry on
            await _next(context);
        }
    }
}