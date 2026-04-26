using System.Diagnostics;

namespace backend.Middleware;

public class LoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<LoggingMiddleware> _logger;

	public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = Stopwatch.StartNew();
		var method = context.Request.Method;
		var path = context.Request.Path;
		var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;

		try
		{
			await _next(context);
			stopwatch.Stop();

			_logger.LogInformation(
				"HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs} ms",
				method,
				path,
				query,
				context.Response.StatusCode,
				stopwatch.ElapsedMilliseconds);
		}
		catch
		{
			stopwatch.Stop();

			_logger.LogWarning(
				"HTTP {Method} {Path}{Query} failed after {ElapsedMs} ms",
				method,
				path,
				query,
				stopwatch.ElapsedMilliseconds);

			throw;
		}
	}
}
