using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Domain.Entities
{
    public class SupportTicket
    {
        [Key]
        public Guid Id { get; set; }

        [Required, EmailAddress, MaxLength(320)]
        public string Email { get; set; } = default!;

        [Required, MaxLength(50)]
        public string Subject { get; set; } = default!;

        [Required]
        public string Description { get; set; } = default!;

        /// <summary>Set to DateTime.UtcNow at creation; used to enforce the 1-ticket-per-email-per-day rule.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
