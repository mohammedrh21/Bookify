using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Domain.Entities
{
    public class FAQ
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Question { get; set; } = default!;
        [Required]
        public string Answer { get; set; } = default!;
    }
}
