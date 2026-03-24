using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.FAQ
{
    public class UpdateFAQRequest
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = default!;
        public string Answer { get; set; } = default!;
    }
}
