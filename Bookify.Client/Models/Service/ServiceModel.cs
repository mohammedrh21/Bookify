using System.ComponentModel.DataAnnotations;

namespace Bookify.Client.Models.Service;

public class ServiceModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public Guid StaffId { get; set; }
    public bool IsDeleted { get; set; }
}
