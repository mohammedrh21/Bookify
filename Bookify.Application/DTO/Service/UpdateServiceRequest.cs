using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Application.DTO.Service
{
    public class UpdateServiceRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        [Required]
        public TimeSpan TimeStart { get; set; }
        [Required]
        public TimeSpan TimeEnd { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
    }
}
