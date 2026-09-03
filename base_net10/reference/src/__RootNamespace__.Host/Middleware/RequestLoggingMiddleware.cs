using System.Text;
using System.Text.RegularExpressions;
using ILogger = Serilog.ILogger;

namespace __RootNamespace__.Host.Middleware;

// Limite de seguridad: bodies mayores a este tamaño se truncan para evitar
// presion extrema de memoria/GC y picos de CPU en endpoints con respuestas grandes.
// El body de respuesta NO se loguea porque implicaria bufferizar la respuesta completa.
public sealed class RequestLoggingMiddleware
{
    private const int MaxBodyLogBytes = 32 * 1024; // 32 KB
    private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromMilliseconds(100);
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = Serilog.Log.ForContext<RequestLoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        var requestBody = SanitizeRequestBody(await FormatRequest(context.Request));
        _logger.Information(
            "Request: {Method} {Path} {QueryString} | Body: {RequestBody}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestBody);

        try
        {
            await _next(context);
        }
        finally
        {
            if (context.Response.StatusCode >= 400 && context.Response.StatusCode < 500)
            {
                _logger.Warning("Response: {StatusCode} | Path: {Path}", context.Response.StatusCode, context.Request.Path);
            }
            else if (context.Response.StatusCode >= 500)
            {
                _logger.Error("Response: {StatusCode} | Path: {Path}", context.Response.StatusCode, context.Request.Path);
            }
            else
            {
                _logger.Information("Response: {StatusCode} | Path: {Path}", context.Response.StatusCode, context.Request.Path);
            }
        }
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        if (request.ContentLength == 0)
            return "[Empty Body]";

        var buffer = new byte[MaxBodyLogBytes];
        int bytesRead;

        try
        {
            bytesRead = await request.Body.ReadAsync(buffer, 0, MaxBodyLogBytes);
            if (bytesRead == 0)
                return "[Empty Body]";

            var body = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var truncated = (request.ContentLength.HasValue && request.ContentLength > MaxBodyLogBytes)
                            || bytesRead == MaxBodyLogBytes;

            return truncated ? $"{body} ... [TRUNCADO]" : body;
        }
        catch (Exception ex)
        {
            return $"[Error reading body: {ex.Message}]";
        }
        finally
        {
            if (request.Body.CanSeek)
                request.Body.Position = 0;
        }
    }

    private static string SanitizeRequestBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body) || body.StartsWith("[", StringComparison.Ordinal))
            return body;

        // JSON keys: "password", "oldPassword", "newPassword"
        body = Regex.Replace(
            body,
            "(?i)(\"(?:password|oldpassword|newpassword)\"\\s*:\\s*\")[^\"]*(\")",
            "$1***$2",
            RegexOptions.None,
            RegexMatchTimeout);

        // Form urlencoded keys: password=..., oldPassword=..., newPassword=...
        body = Regex.Replace(
            body,
            "(?i)((?:^|[&?])(?:password|oldpassword|newpassword)=)[^&]*",
            "$1***",
            RegexOptions.None,
            RegexMatchTimeout);

        return body;
    }
}
