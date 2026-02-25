using System;
using System.Collections.Generic;
using System.Text;

namespace Bookify.Application.DTO.Service
{
    public class CreateServiceRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public Guid StaffId { get; set; }
        public Guid CategoryId { get; set; }
    }
}
