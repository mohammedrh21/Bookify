using Bookify.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Bookify.API.Middleware
{
    /// <summary>
    /// Catches all unhandled exceptions and returns a consistent, RFC-7807-style error response.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        private static bool _isDevelopment;

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
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Cannot handle exception – response already started.");
                throw exception;
            }

            var errorResponse = BuildErrorResponse(context, exception);
            LogError(context, exception, errorResponse);

            context.Response.Clear();
            context.Response.StatusCode = errorResponse.StatusCode;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }

        private ErrorResponse BuildErrorResponse(HttpContext context, Exception exception)
        {
            var (statusCode, errorType, message, details) = ClassifyException(exception);

            var response = new ErrorResponse
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

            if (_environment.IsDevelopment())
            {
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            return response;
        }

        private static (HttpStatusCode status, string type, string message, string? details)
            ClassifyException(Exception exception) => exception switch
            {
                // ── Domain exceptions (custom, always safe to expose) ──────────────────
                NotFoundException notFound => (
                    HttpStatusCode.NotFound,
                    "NotFound",
                    notFound.Message,
                    null),

                ConflictException conflict => (
                    HttpStatusCode.Conflict,
                    "Conflict",
                    conflict.Message,
                    null),

                BusinessRuleException rule => (
                    HttpStatusCode.UnprocessableEntity,
                    "BusinessRuleViolation",
                    rule.Message,
                    null),

                ForbiddenException forbidden => (
                    HttpStatusCode.Forbidden,
                    "Forbidden",
                    forbidden.Message,
                    null),

                InvalidBookingTransitionException transition => (
                    HttpStatusCode.UnprocessableEntity,
                    "InvalidStateTransition",
                    transition.Message,
                    null),

                TimeSlotUnavailableException slot => (
                    HttpStatusCode.Conflict,
                    "TimeSlotUnavailable",
                    slot.Message,
                    null),

                // ── Infrastructure / EF exceptions ────────────────────────────────────
                DbUpdateConcurrencyException concurrency => (
                    HttpStatusCode.Conflict,
                    "ConcurrencyError",
                    "The record was modified by another user. Please refresh and try again.",
                    $"Affected entries: {concurrency.Entries.Count}"),

                DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx => (
                    GetSqlStatusCode(sqlEx),
                    "DatabaseError",
                    "A database error occurred.",
                    GetSqlDetails(sqlEx)),

                // ── Standard .NET exceptions ───────────────────────────────────────────
                ArgumentNullException argNull => (
                    HttpStatusCode.BadRequest,
                    "ArgumentNull",
                    argNull.Message,
                    null),

                ArgumentException arg => (
                    HttpStatusCode.BadRequest,
                    "ArgumentError",
                    arg.Message,
                    null),

                KeyNotFoundException key => (
                    HttpStatusCode.NotFound,
                    "NotFound",
                    key.Message,
                    null),

                NotImplementedException notImpl => (
                    HttpStatusCode.NotImplemented,
                    "NotImplemented",
                    "This feature is not yet implemented.",
                    notImpl.Message),

                OperationCanceledException => (
                    HttpStatusCode.RequestTimeout,
                    "RequestCancelled",
                    "The request was cancelled.",
                    null),

                // ── Catch-all ─────────────────────────────────────────────────────────
                _ => (
                    HttpStatusCode.InternalServerError,
                    "InternalServerError",
                    "An unexpected error occurred. Please try again later.",
                    null)
            };

        private static HttpStatusCode GetSqlStatusCode(SqlException ex) => ex.Number switch
        {
            547 or 2601 or 2627 => HttpStatusCode.Conflict,
            -2 or -2146232060 => HttpStatusCode.RequestTimeout,
            4060 or 18456 => HttpStatusCode.ServiceUnavailable,
            _ => HttpStatusCode.InternalServerError
        };

        private static string GetSqlDetails(SqlException ex) => ex.Number switch
        {
            2627 => "A record with this identifier already exists.",
            2601 => "A record with this value already exists.",
            547 => "Cannot complete: record is referenced by other data.",
            515 => "Cannot insert NULL into a required field.",
            8152 => "The provided data is too long for the target field.",
            1205 => "A database deadlock occurred. Please retry the operation.",
            208 => "Database table not found. Run 'dotnet ef database update'.",
            _ => $"SQL error {ex.Number}."
        };

        private void LogError(HttpContext context, Exception exception, ErrorResponse response)
        {
            var level = response.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            _logger.Log(
                level,
                exception,
                "Unhandled exception. TraceId: {TraceId} | Status: {StatusCode} | Type: {ErrorType} | " +
                "{Method} {Path} | User: {User}",
                response.TraceId,
                response.StatusCode,
                response.ErrorType,
                response.Method,
                response.Path,
                context.User?.Identity?.Name ?? "Anonymous");
        }

        public static void SetEnvironment(bool isDevelopment) => _isDevelopment = isDevelopment;
    }

    /// <summary>RFC-7807-inspired error response model.</summary>
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorType { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Details { get; set; }
        public string Path { get; set; } = default!;
        public string Method { get; set; } = default!;
        public string TraceId { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public string Instance { get; set; } = default!;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder app,
            IHostEnvironment environment)
        {
            GlobalExceptionMiddleware.SetEnvironment(environment.IsDevelopment());
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
