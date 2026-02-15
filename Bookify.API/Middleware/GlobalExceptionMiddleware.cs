using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Bookify.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware with database exception support
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Don't try to modify response if it's already started
            if (context.Response.HasStarted)
            {
                throw new InvalidOperationException(
                    "The response has already started, the exception middleware will not be executed.",
                    exception);
            }

            var (statusCode, message, details) = GetExceptionDetails(exception);

            var response = new
            {
                StatusCode = (int)statusCode,
                Message = message,
                Details = details,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            return context.Response.WriteAsync(json);
        }

        private static (HttpStatusCode statusCode, string message, string? details) GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
                // Database Exceptions - Most Common
                DbUpdateException dbEx when dbEx.InnerException is SqlException sqlEx => (
                    HttpStatusCode.InternalServerError,
                    "A database error occurred. Please check the configuration.",
                    GetSqlExceptionDetails(sqlEx)
                ),

                DbUpdateConcurrencyException => (
                    HttpStatusCode.Conflict,
                    "The record you attempted to update was modified by another user. Please refresh and try again.",
                    null
                ),

                DbUpdateException dbEx => (
                    HttpStatusCode.InternalServerError,
                    "A database error occurred while saving your changes.",
                    dbEx.InnerException?.Message
                ),

                // SQL Specific Exceptions
                SqlException sqlEx => (
                    HttpStatusCode.InternalServerError,
                    "A database connection error occurred.",
                    GetSqlExceptionDetails(sqlEx)
                ),

                // Application Exceptions
                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "The requested resource was not found.",
                    null
                ),

                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "You are not authorized to access this resource.",
                    null
                ),

                ArgumentException or ArgumentNullException => (
                    HttpStatusCode.BadRequest,
                    exception.Message,
                    null
                ),

                InvalidOperationException => (
                    HttpStatusCode.BadRequest,
                    exception.Message,
                    null
                ),

                // Generic Exception
                _ => (
                    HttpStatusCode.InternalServerError,
                    "An error occurred processing your request. Please try again later.",
                    exception.Message
                )
            };
        }

        private static string GetSqlExceptionDetails(SqlException sqlEx)
        {
            return sqlEx.Number switch
            {
                // Cannot open database
                4060 => "Cannot connect to the database. Please check the connection string.",

                // Login failed
                18456 => "Database authentication failed. Please check the credentials.",

                // Server not found
                -1 or -2 => "Database server not found. Please check the server name.",

                // Timeout
                -2146232060 => "Database connection timeout. Please try again.",

                // Invalid object name (table doesn't exist)
                208 => $"Database table not found: {sqlEx.Message}. Please run database migrations using 'dotnet ef database update'.",

                // Primary key violation
                2627 => "A record with this identifier already exists.",

                // Foreign key violation
                547 => "Cannot delete or update a record because it is referenced by other data.",

                // Unique constraint violation
                2601 => "A record with this value already exists.",

                _ => $"SQL Error {sqlEx.Number}: {sqlEx.Message}"
            };
        }
    }

    /// <summary>
    /// Extension method to register the global exception middleware
    /// </summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}

