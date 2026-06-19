/*
 * Day 12 — Middleware Fundamentals
 *
 * Problem:
 * Write a simple middleware that logs every request's HTTP method and path
 * before passing control to the next step in the pipeline.
 *
 * This is the foundation pattern used later for GlobalExceptionMiddleware.
 */

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation(
            "HTTP Method: {HttpMethod} Path: {HttpPath}",
            context.Request.Method,
            context.Request.Path);

        await _next(context);
    }
}

/*
 * ─────────────────────────────────────────────
 * Middleware — what, why, when
 * ─────────────────────────────────────────────
 *
 * WHAT is middleware?
 * A component that sits in the HTTP request pipeline. Every request passes
 * through every registered middleware, in the order they're registered in
 * Program.cs, before reaching the controller — and back out again in reverse
 * order on the way to the response.
 *
 * THE CONSTRUCTOR PATTERN — runs ONCE per app lifetime:
 *   RequestDelegate _next is injected once when the app starts.
 *   It represents "whatever middleware comes after me."
 *
 * THE InvokeAsync METHOD — runs ONCE PER REQUEST:
 *   This is where you read context.Request, do your logic,
 *   then call await _next(context) to pass control onward.
 *
 * Common mistake: forgetting to call _next(context) — this means
 * the pipeline stops here and the controller never runs.
 *
 * ─────────────────────────────────────────────
 * Registering middleware in Program.cs
 * ─────────────────────────────────────────────
 *
 *   app.UseMiddleware<RequestLoggingMiddleware>();
 *
 * ORDER MATTERS. Middleware registered earlier wraps middleware
 * registered later, like layers of an onion:
 *
 *   app.UseMiddleware<GlobalExceptionMiddleware>();  // outermost — catches everything
 *   app.UseHttpsRedirection();
 *   app.UseAuthorization();
 *   app.MapControllers();                            // innermost — your actual endpoint
 *
 * If GlobalExceptionMiddleware is registered AFTER MapControllers,
 * it would never catch exceptions thrown inside a controller — too late,
 * the pipeline never reaches back out to it.
 *
 * ─────────────────────────────────────────────
 * This pattern leads directly to GlobalExceptionMiddleware
 * ─────────────────────────────────────────────
 *
 * Same shape, but wraps _next(context) in try/catch instead of just logging:
 *
 *   try { await _next(context); }
 *   catch (ValidationException ex) { ... return 400 ... }
 *   catch (Exception ex)           { ... return 500 ... }
 *
 * Because middleware wraps everything downstream, it catches exceptions
 * thrown anywhere in the controller, service, or repository layers —
 * one place to handle every unhandled exception in the whole API.
 */
