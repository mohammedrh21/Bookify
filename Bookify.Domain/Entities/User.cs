using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Domain.Entities
{
    // =====================================
    // Abstract User
    // =====================================
    public abstract class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        public string Phone { get; set; } = default!;
    }

    // =====================================
    // Client (End-user)
    // =====================================
    public class Client : User
    {
        public DateTime? DateOfBirth { get; set; }

        // One client can have multiple bookings
        public ICollection<Booking>? Bookings { get; set; }
    }

    // =====================================
    // Staff (handles services)
    // =====================================
    public class Staff : User
    {
        // One-to-One with Service
        public Service Service { get; set; } = default!;
    }

    // =====================================
    // Admin (system administrator)
    // =====================================
    public class Admin : User
    {
        // Reserved for future admin logic
    }
}
