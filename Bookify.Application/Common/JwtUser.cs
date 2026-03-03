using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Common
{
    public class JwtUser
    {
        public Guid Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string UserName { get; set; } = default!;
    }
}
