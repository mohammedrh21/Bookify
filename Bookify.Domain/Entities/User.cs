using Bookify.Domain.Enums;
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
        [StringLength(15, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 15 characters.")]
        public string Phone { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        // Navigation: user notifications
        public ICollection<Notification>? Notifications { get; set; }
    }

    // =====================================
    // Client (End-user)
    // =====================================
    public class Client : User
    {
        public GenderType Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImagePath { get; set; }

        // One client can have multiple bookings
        public ICollection<Booking>? Bookings { get; set; }
    }

    // =====================================
    // Staff (handles services)
    // =====================================
    public class Staff : User
    {
        public GenderType Gender { get; set; }
        public string? ImagePath { get; set; }
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
