using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bookify.API.Filters;

/// <summary>
/// A global action filter that automatically validates any [FromBody] or [FromForm]
/// action parameters against their registered FluentValidation <see cref="IValidator{T}"/>.
/// Returns a consistent 400 response when validation fails; passes through when no validator
/// is registered (non-breaking for endpoints that don't need one).
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    private readonly ILogger<ValidationFilter> _logger;

    public ValidationFilter(ILogger<ValidationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Collect all [FromBody] / [FromForm] parameters that have a bound value
        var requestParams = context.ActionDescriptor.Parameters
            .Where(p =>
            {
                // Only validate parameters that come from the body or form
                if (p is Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor cpd)
                {
                    var bindingSource = cpd.ParameterInfo
                        .GetCustomAttributes(inherit: true)
                        .OfType<IBindingSourceMetadata>()
                        .Select(bsm => bsm.BindingSource)
                        .FirstOrDefault();

                    // If explicitly marked [FromQuery], [FromRoute] etc. → skip
                    if (bindingSource != null &&
                        bindingSource != Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body &&
                        bindingSource != Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Form)
                        return false;
                }
                return true;
            })
            .ToList();

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var param in requestParams)
        {
            if (!context.ActionArguments.TryGetValue(param.Name, out var argument) || argument is null)
                continue;

            var paramType = param.ParameterType;

            // Resolve IValidator<T> for this parameter type
            var validatorType = typeof(IValidator<>).MakeGenericType(paramType);
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator is null)
                continue; // No validator registered — pass through

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                foreach (var failure in result.Errors)
                {
                    var key = ToCamelCase(failure.PropertyName);

                    if (errors.TryGetValue(key, out var existing))
                        errors[key] = [.. existing, failure.ErrorMessage];
                    else
                        errors[key] = [failure.ErrorMessage];
                }
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Validation failed for {Action} on {Controller}. Errors: {ErrorCount}",
                context.RouteData.Values["action"],
                context.RouteData.Values["controller"],
                errors.Values.Sum(e => e.Length));

            context.Result = new BadRequestObjectResult(new ValidationErrorResponse
            {
                StatusCode  = StatusCodes.Status400BadRequest,
                ErrorType   = "ValidationError",
                Message     = "One or more validation errors occurred.",
                Errors      = errors,
                Path        = context.HttpContext.Request.Path,
                Method      = context.HttpContext.Request.Method,
                TraceId     = context.HttpContext.TraceIdentifier,
                Timestamp   = DateTime.UtcNow,
                Instance    = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            });
            return;
        }

        await next();
    }

    /// <summary>Converts "PropertyName" → "propertyName" for consistent JSON keys.</summary>
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

/// <summary>
/// RFC-7807-aligned validation error response, consistent with the existing
/// <see cref="Bookify.API.Middleware.ErrorResponse"/> shape.
/// </summary>
public sealed class ValidationErrorResponse
{
    public int StatusCode   { get; init; }
    public string ErrorType { get; init; } = default!;
    public string Message   { get; init; } = default!;

    /// <summary>
    /// Each key is a camelCase field name; each value is an array of violation messages.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; init; } = new();

    public string Path      { get; init; } = default!;
    public string Method    { get; init; } = default!;
    public string TraceId   { get; init; } = default!;
    public DateTime Timestamp { get; init; }
    public string Instance  { get; init; } = default!;
}
