using System;
using System.Collections.Generic;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Review;

namespace Bookify.Client.Models.Common
{
    public class DashboardReportResponse
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedBookings { get; set; }
        public int TotalServices { get; set; }
        public double TotalRevenue { get; set; }
        
        // Trends (Percentage Change)
        public double TotalBookingsTrend { get; set; }
        public double PendingBookingsTrend { get; set; }
        public double TotalRevenueTrend { get; set; }
        public double TotalServicesTrend { get; set; }

        // For Charts
        public List<ChartDataPoint> BookingTrends { get; set; } = [];
        public List<ChartDataPoint> CategoryDistribution { get; set; } = [];
        
        // Smart Insights
        public List<DashboardInsightDto> Insights { get; set; } = [];

        // Performance
        public List<StaffPerformanceDto> TopStaff { get; set; } = [];
        public List<ServicePerformanceDto> MostBookedServices { get; set; } = [];

        // Latest Activity
        public List<RecentActivityDto> RecentActivities { get; set; } = [];
        public List<BookingModel> RecentBookings { get; set; } = [];

        // Reviews (Staff Dashboard specific properties)
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<int> RatingDistribution { get; set; } = [0, 0, 0, 0, 0];
        public List<ReviewModel> RecentReviews { get; set; } = [];
    }

    public class DashboardInsightDto
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Icon { get; set; } = "lightbulb";
        public string Color { get; set; } = "blue";
    }

    public class StaffPerformanceDto
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = default!;
        public int CompletedBookings { get; set; }
        public double Revenue { get; set; }
        public double Rating { get; set; }
    }

    public class ServicePerformanceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = default!;
        public int BookingCount { get; set; }
        public double Revenue { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = default!;
        public double Value { get; set; }
    }

    public class RecentActivityDto
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = "Info";
    }
}
