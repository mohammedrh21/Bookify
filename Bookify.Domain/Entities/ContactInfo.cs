using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookify.Domain.Entities
{
    public class ContactInfo
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Country { get; set; } = default!;
        public string? AddressLine_1 { get; set; } = string.Empty;
        public string? AddressLine_2 { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = default!;
        [Required, Phone]
        public string PhoneNumber { get; set; } = default!;
        [Required]
        public TimeSpan CallHourFrom { get; set; }
        [Required]
        public TimeSpan CallHourTo { get; set; }
        [Required, Range(0, 6)]
        public DayOfWeek CallDayFrom { get; set; }
        [Required, Range(0, 6)]
        public DayOfWeek CallDayTo { get; set; }
    }
}
