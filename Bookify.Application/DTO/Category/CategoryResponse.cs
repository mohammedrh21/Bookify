namespace Bookify.Application.DTO.Category
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public int ServiceCount { get; set; }
    }
}
