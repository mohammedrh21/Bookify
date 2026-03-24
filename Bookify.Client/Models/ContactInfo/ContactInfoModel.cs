using System.Text.Json.Serialization;

namespace Bookify.Client.Models.ContactInfo;

public class ContactInfoModel
{
    public Guid Id { get; set; }
    public string Country { get; set; } = default!;
    public string? AddressLine_1 { get; set; } = string.Empty;
    public string? AddressLine_2 { get; set; } = string.Empty;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public TimeSpan CallHourFrom { get; set; }
    public TimeSpan CallHourTo { get; set; }
    public DayOfWeek CallDayFrom { get; set; }
    public DayOfWeek CallDayTo { get; set; }
}
