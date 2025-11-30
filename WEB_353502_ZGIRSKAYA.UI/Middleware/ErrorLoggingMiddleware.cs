using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WEB_353502_ZGIRSKAYA.UI.Middleware
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;

            if (statusCode < 200 || statusCode >= 300)
            {
                var path = context.Request.Path + context.Request.QueryString;
                _logger.LogInformation("---> request {Path} returns {StatusCode}", path, statusCode);
            }
        }
    }
}