using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO
{
    public abstract class RegisterBaseRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;
    }
}
