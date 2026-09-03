namespace __RootNamespace__.Host.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

        await _next(context);
    }
}
