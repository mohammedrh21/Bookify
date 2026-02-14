using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    /// <summary>
    /// Represents a bookable service handled by a single staff member
    /// </summary>
    public class Service
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        [Required]
        public int Duration { get; set; }     // Minutes

        [Required]
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; } = false;

        // One-to-One: Each service is handled by one staff
        public Guid StaffId { get; set; }
        public Staff Staff { get; set; } = default!;

        // Many-to-One: Each service belongs to a category
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // One-to-Many: A service can have multiple bookings
        public ICollection<Booking>? Bookings { get; set; }
    }
}

