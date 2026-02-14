using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Interfaces
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(Exception exception, string message);
    }
}
