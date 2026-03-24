using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.FAQ
{
    public class CreateFAQRequest
    {
        public string Question { get; set; } = default!;
        public string Answer { get; set; } = default!;
    }
}
