using Bookify.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Bookify.Domain.Entities
{
    public class ServiceApprovalRequest
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ServiceId { get; set; } // Null for Create, populate for Update
        public Service? Service { get; set; }

        [Required]
        public Guid StaffId { get; set; }
        public Staff Staff { get; set; } = default!;

        [Required]
        public ApprovalRequestType Type { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required]
        public string ProposedData { get; set; } = default!; // JSON encoded Create/Update Request

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ActionedAt { get; set; }
        
        public Guid? ActionedBy { get; set; } // Admin ID
        public Admin? Actioner { get; set; }

        public string? AdminComment { get; set; }
    }
}
