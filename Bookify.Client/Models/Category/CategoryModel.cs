namespace Bookify.Client.Models.Category;

public class CategoryModel
{
    public Guid   Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;
    public string Icon         { get; set; } = "📋";
    public int    ServiceCount { get; set; }
}
