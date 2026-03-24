using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Application.DTO.ContactInfo
{
    public class CreateContactInfoRequest
    {
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
}
