using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        // One category can have multiple services
        public ICollection<Service>? Services { get; set; }
    }
}
