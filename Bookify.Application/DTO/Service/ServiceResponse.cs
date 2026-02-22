namespace Bookify.Application.DTO.Service
{
    public class ServiceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int Duration { get; set; } // minutes
        public Guid StaffId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public string StaffName { get; set; } = default!;
        public bool IsDeleted { get; set; }
    }
}
