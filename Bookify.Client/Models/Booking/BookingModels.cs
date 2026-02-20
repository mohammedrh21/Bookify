namespace Bookify.Client.Models.Booking;

public class BookingModel
{
    public Guid     Id              { get; set; }
    public Guid     ServiceId       { get; set; }
    public string   ServiceName     { get; set; } = string.Empty;
    public string   StaffName       { get; set; } = string.Empty;
    public string   ClientName      { get; set; } = string.Empty;
    public DateTime Date            { get; set; }
    public TimeSpan Time            { get; set; }
    public string   Status          { get; set; } = string.Empty;
    public decimal  Price           { get; set; }
    public int      DurationMinutes { get; set; }
}

public class CreateBookingRequest
{
    public Guid     ClientId  { get; set; }
    public Guid     ServiceId { get; set; }
    public Guid     StaffId   { get; set; }
    public DateTime Date      { get; set; }
    public TimeSpan Time      { get; set; }
}
