using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Bookify.API.Middleware
{
    /// <summary>
    /// Unified exception handling middleware that catches ALL exceptions and returns consistent responses
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Don't try to modify response if it's already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "Cannot handle exception - response has already started. Exception: {Message}",
                    exception.Message);
                throw exception;
            }

            // Get error details
            var errorResponse = CreateErrorResponse(context, exception);

            // Log the error with full details
            LogError(context, exception, errorResponse);

            // Set response
            context.Response.Clear();
            context.Response.StatusCode = errorResponse.StatusCode;
            context.Response.ContentType = "application/json";

            // Serialize and write response
            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }

        private ErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
        {
            var (statusCode, errorType, message, details) = GetErrorDetails(exception);

            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                ErrorType = errorType,
                Message = message,
                Details = details,
                Path = context.Request.Path,
                Method = context.Request.Method,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };

            // Add stack trace only in development
            if (_environment.IsDevelopment())
            {
                errorResponse.StackTrace = exception.StackTrace;
                errorResponse.InnerException = exception.InnerException?.Message;
            }

            return errorResponse;
        }

        private static (HttpStatusCode statusCode, string errorType, string message, string? details) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                // ============================================
                // Database Exceptions
                // ============================================
                DbUpdateConcurrencyException concurrencyEx => (
                    HttpStatusCode.Conflict,
                    "ConcurrencyError",
                    "The record was modified by another user. Please refresh and try again.",
                    $"Affected entries: {concurrencyEx.Entries.Count}"
                ),

                DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx => (
                    GetSqlExceptionStatusCode(sqlEx),
                    "DatabaseError",
                    "A database error occurred.",
                    GetSqlExceptionDetails(sqlEx)
                ),

                DbUpdateException dbEx => (
                    HttpStatusCode.InternalServerError,
                    "DatabaseError",
                    "A database error occurred while saving changes.",
                    dbEx.InnerException?.Message
                ),

                SqlException sqlEx => (
                    GetSqlExceptionStatusCode(sqlEx),
                    "DatabaseConnectionError",
                    "Failed to connect to the database.",
                    GetSqlExceptionDetails(sqlEx)
                ),

                // ============================================
                // HTTP Exceptions (if using HttpRequestException)
                // ============================================
                HttpRequestException httpEx => (
                    httpEx.StatusCode ?? HttpStatusCode.ServiceUnavailable,
                    "ExternalServiceError",
                    "Failed to communicate with external service.",
                    httpEx.Message
                ),

                // ============================================
                // Authorization & Authentication
                // ============================================
                UnauthorizedAccessException unauthorizedEx => (
                    HttpStatusCode.Forbidden,
                    "Forbidden",
                    "You do not have permission to access this resource.",
                    unauthorizedEx.Message
                ),

                // ============================================
                // Validation Exceptions
                // ============================================
                ArgumentNullException argNullEx => (
                    HttpStatusCode.BadRequest,
                    "ValidationError",
                    $"Required parameter is missing: {argNullEx.ParamName}",
                    argNullEx.Message
                ),

                ArgumentException argEx => (
                    HttpStatusCode.BadRequest,
                    "ValidationError",
                    "Invalid argument provided.",
                    argEx.Message
                ),

                InvalidOperationException invalidOpEx => (
                    HttpStatusCode.BadRequest,
                    "InvalidOperation",
                    "The operation is not valid in the current state.",
                    invalidOpEx.Message
                ),

                // ============================================
                // Not Found
                // ============================================
                KeyNotFoundException notFoundEx => (
                    HttpStatusCode.NotFound,
                    "NotFound",
                    "The requested resource was not found.",
                    notFoundEx.Message
                ),

                FileNotFoundException fileNotFoundEx => (
                    HttpStatusCode.NotFound,
                    "FileNotFound",
                    "The requested file was not found.",
                    fileNotFoundEx.FileName
                ),

                // ============================================
                // Timeout Exceptions
                // ============================================
                TimeoutException timeoutEx => (
                    HttpStatusCode.RequestTimeout,
                    "Timeout",
                    "The operation timed out.",
                    timeoutEx.Message
                ),

                OperationCanceledException canceledEx => (
                    HttpStatusCode.RequestTimeout,
                    "OperationCanceled",
                    "The operation was canceled.",
                    canceledEx.Message
                ),

                // ============================================
                // Format & Parsing Exceptions
                // ============================================
                FormatException formatEx => (
                    HttpStatusCode.BadRequest,
                    "FormatError",
                    "Invalid format provided.",
                    formatEx.Message
                ),

                JsonException jsonEx => (
                    HttpStatusCode.BadRequest,
                    "JsonParseError",
                    "Failed to parse JSON data.",
                    jsonEx.Message
                ),

                // ============================================
                // IO Exceptions
                // ============================================
                IOException ioEx => (
                    HttpStatusCode.InternalServerError,
                    "IOError",
                    "An I/O error occurred.",
                    ioEx.Message
                ),

                // ============================================
                // Null Reference
                // ============================================
                NullReferenceException nullRefEx => (
                    HttpStatusCode.InternalServerError,
                    "NullReference",
                    "An unexpected null reference was encountered.",
                    _isDevelopment ? nullRefEx.Message : "Internal server error"
                ),

                // ============================================
                // Not Implemented
                // ============================================
                NotImplementedException notImplEx => (
                    HttpStatusCode.NotImplemented,
                    "NotImplemented",
                    "This functionality is not yet implemented.",
                    notImplEx.Message
                ),

                // ============================================
                // Not Supported
                // ============================================
                NotSupportedException notSuppEx => (
                    HttpStatusCode.BadRequest,
                    "NotSupported",
                    "This operation is not supported.",
                    notSuppEx.Message
                ),

                // ============================================
                // Generic Exception (Catch-all)
                // ============================================
                _ => (
                    HttpStatusCode.InternalServerError,
                    "InternalServerError",
                    "An unexpected error occurred. Please try again later.",
                    _isDevelopment ? exception.Message : null
                )
            };
        }

        private static HttpStatusCode GetSqlExceptionStatusCode(SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                // Constraint violations
                547 or 2601 or 2627 => HttpStatusCode.Conflict,

                // Invalid object name (table doesn't exist)
                208 => HttpStatusCode.InternalServerError,

                // Timeout
                -2 => HttpStatusCode.RequestTimeout,

                // Cannot open database, login failed
                4060 or 18456 => HttpStatusCode.ServiceUnavailable,

                _ => HttpStatusCode.InternalServerError
            };
        }

        private static string GetSqlExceptionDetails(SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                // Cannot open database
                4060 => "Cannot connect to the database. Please check the connection string.",

                // Login failed
                18456 => "Database authentication failed. Please verify your credentials.",

                // Server not found or network error
                -1 or -2 => "Database server is unreachable. Please check the server name and network connection.",

                // Timeout
                -2146232060 => "Database operation timed out. Please try again.",

                // Invalid object name (table doesn't exist)
                208 => $"Database table not found: {sqlEx.Message}. Run 'dotnet ef database update' to apply migrations.",

                // Primary key violation
                2627 => "A record with this identifier already exists. Please use a unique identifier.",

                // Foreign key violation
                547 => "Cannot complete operation: this record is referenced by other data. Delete dependent records first.",

                // Unique constraint violation
                2601 => "A record with this value already exists. Please use a unique value.",

                // Deadlock
                1205 => "A database deadlock occurred. The operation has been rolled back. Please try again.",

                // Cannot insert NULL
                515 => "Cannot insert NULL value into a required field.",

                // String or binary data truncated
                8152 => "The data provided is too long for the database field.",

                _ => $"SQL Error {sqlEx.Number}: {sqlEx.Message}"
            };
        }

        private void LogError(HttpContext context, Exception exception, ErrorResponse errorResponse)
        {
            var logLevel = errorResponse.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            _logger.Log(
                logLevel,
                exception,
                "Unhandled exception occurred. " +
                "TraceId: {TraceId}, " +
                "StatusCode: {StatusCode}, " +
                "ErrorType: {ErrorType}, " +
                "Path: {Path}, " +
                "Method: {Method}, " +
                "User: {User}",
                errorResponse.TraceId,
                errorResponse.StatusCode,
                errorResponse.ErrorType,
                errorResponse.Path,
                errorResponse.Method,
                context.User?.Identity?.Name ?? "Anonymous"
            );
        }

        private static bool _isDevelopment = false;

        public static void SetEnvironment(bool isDevelopment)
        {
            _isDevelopment = isDevelopment;
        }
    }

    /// <summary>
    /// Unified error response model
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Error type/category (e.g., "ValidationError", "DatabaseError")
        /// </summary>
        public string ErrorType { get; set; } = default!;

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        /// Additional error details (optional, may be null in production)
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Request path that caused the error
        /// </summary>
        public string Path { get; set; } = default!;

        /// <summary>
        /// HTTP method (GET, POST, etc.)
        /// </summary>
        public string Method { get; set; } = default!;

        /// <summary>
        /// Unique trace ID for this request (for debugging)
        /// </summary>
        public string TraceId { get; set; } = default!;

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// RFC 7807 Problem Details instance identifier
        /// </summary>
        public string Instance { get; set; } = default!;

        /// <summary>
        /// Stack trace (only in development environment)
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception message (only in development environment)
        /// </summary>
        public string? InnerException { get; set; }
    }

    /// <summary>
    /// Extension methods for registering the unified exception handler
    /// </summary>
    public static class UnifiedExceptionHandlerExtensions
    {
        /// <summary>
        /// Adds the unified exception handler middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder app,
            IHostEnvironment environment)
        {
            GlobalExceptionMiddleware.SetEnvironment(environment.IsDevelopment());
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }

        /// <summary>
        /// Alternative: Use built-in exception handler with custom response
        /// </summary>
        public static IApplicationBuilder UseUnifiedExceptionHandlerAlternative(
            this IApplicationBuilder app,
            IHostEnvironment environment)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandlerFeature?.Error;

                    if (exception == null)
                        return;

                    var logger = context.RequestServices
                        .GetRequiredService<ILogger<GlobalExceptionMiddleware>>();

                    var (statusCode, errorType, message, details) =
                        GetErrorDetailsStatic(exception, environment.IsDevelopment());

                    var errorResponse = new ErrorResponse
                    {
                        StatusCode = (int)statusCode,
                        ErrorType = errorType,
                        Message = message,
                        Details = details,
                        Path = context.Request.Path,
                        Method = context.Request.Method,
                        TraceId = context.TraceIdentifier,
                        Timestamp = DateTime.UtcNow,
                        Instance = $"{context.Request.Method} {context.Request.Path}"
                    };

                    if (environment.IsDevelopment())
                    {
                        errorResponse.StackTrace = exception.StackTrace;
                        errorResponse.InnerException = exception.InnerException?.Message;
                    }

                    logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

                    context.Response.StatusCode = errorResponse.StatusCode;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(errorResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });
                });
            });

            return app;
        }

        private static (HttpStatusCode, string, string, string?) GetErrorDetailsStatic(
            Exception exception,
            bool isDevelopment)
        {
            // Same logic as in the middleware
            return exception switch
            {
                DbUpdateConcurrencyException => (
                    HttpStatusCode.Conflict,
                    "ConcurrencyError",
                    "The record was modified by another user.",
                    null
                ),
                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "NotFound",
                    "Resource not found.",
                    null
                ),
                ArgumentException => (
                    HttpStatusCode.BadRequest,
                    "ValidationError",
                    exception.Message,
                    null
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "InternalServerError",
                    "An unexpected error occurred.",
                    isDevelopment ? exception.Message : null
                )
            };
        }
    }
}
