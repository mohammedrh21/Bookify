using Bookify.Application.Interfaces;
using Microsoft.Extensions.Logging;


namespace Bookify.Infrastructure.Service
{
    public class LoggerAdapter<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public LoggerAdapter(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogError(Exception exception, string message)
        {
            _logger.LogError(exception, message);
        }
    }
}
