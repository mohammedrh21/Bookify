using Bookify.Domain.Enums;
using System;

namespace Bookify.Application.DTO.Service
{
    public class ServiceApprovalRequestResponse
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = default!;
        public ApprovalRequestType Type { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public ServiceApprovalProposedDetailsDto ProposedDetails { get; set; } = default!;
        public ServiceResponse? CurrentDetails { get; set; }
        public string? AdminComment { get; set; }
    }

    public class ServiceApprovalProposedDetailsDto
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
    }
}
