using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bookify.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                // Prevent MIME-sniffing
                if (!headers.ContainsKey("X-Content-Type-Options"))
                {
                    headers.Append("X-Content-Type-Options", "nosniff");
                }

                // Prevent clickjacking
                if (!headers.ContainsKey("X-Frame-Options"))
                {
                    headers.Append("X-Frame-Options", "DENY");
                }

                // XSS Protection (older browsers)
                if (!headers.ContainsKey("X-XSS-Protection"))
                {
                    headers.Append("X-XSS-Protection", "1; mode=block");
                }

                // Strictly control referer information sent to other sites
                if (!headers.ContainsKey("Referrer-Policy"))
                {
                    headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                }

                // Prevent browser from sending this data to HTTP
                if (!headers.ContainsKey("Strict-Transport-Security"))
                {
                    // 1 year
                    headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
                }

                // Basic CSP - restrict things to the current origin where possible
                // Adjusted for typical API usage (allowing some styles/scripts if swagger is used in dev)
                if (!headers.ContainsKey("Content-Security-Policy"))
                {
                    headers.Append("Content-Security-Policy", "default-src 'self'; img-src 'self' data: https:; font-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; frame-ancestors 'none';");
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
