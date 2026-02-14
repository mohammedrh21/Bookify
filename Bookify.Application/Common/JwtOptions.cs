using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.Common
{
    public class JwtOptions
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int DurationInMinutes { get; set; }
    }
}
