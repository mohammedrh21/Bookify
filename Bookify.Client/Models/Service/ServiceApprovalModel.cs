using System;
using System.Text.Json.Serialization;

namespace Bookify.Client.Models.Service
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApprovalRequestType
    {
        Create = 1,
        Update = 2
    }

    public class ServiceApprovalRequestModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public ApprovalRequestType Type { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AdminComment { get; set; }
        public ServiceApprovalProposedDetailsModel ProposedDetails { get; set; } = new();
        public ServiceModel? CurrentDetails { get; set; }
    }

    public class ServiceApprovalProposedDetailsModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}

