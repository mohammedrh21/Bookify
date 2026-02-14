using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Common
{
    public record ServiceResponse<T>(
      bool Success,
      string? Message = null,
      T? Data = default,
      Guid? Id = null)
    {
        public static ServiceResponse<T> Ok(
            T? data = default,
            string? message = null,
            Guid? id = null)
            => new(true, message, data, id);

        public static ServiceResponse<T> Fail(string message)
            => new(false, message);
    }
}
