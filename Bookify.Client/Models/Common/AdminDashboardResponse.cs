using Bookify.Client.Models.Booking;

namespace Bookify.Client.Models.Common
{
    /// <summary>
    /// Client-side mirror of the API's AdminDashboardResponse.
    /// Contains platform-wide KPIs, charts, top-staff performance, and insights.
    /// </summary>
    public class AdminDashboardResponse
    {
        // ── KPIs ────────────────────────────────────────────────────────────
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedBookings { get; set; }
        public int TotalServices { get; set; }
        public double TotalRevenue { get; set; }

        // ── Trends (% change vs previous period) ────────────────────────────
        public double TotalBookingsTrend { get; set; }
        public double PendingBookingsTrend { get; set; }
        public double TotalRevenueTrend { get; set; }
        public double TotalServicesTrend { get; set; }

        // ── Charts ──────────────────────────────────────────────────────────
        public List<ChartDataPoint> BookingTrends { get; set; } = [];
        public List<ChartDataPoint> CategoryDistribution { get; set; } = [];

        // ── Performance ─────────────────────────────────────────────────────
        public List<StaffPerformanceDto> TopStaff { get; set; } = [];
        public List<ServicePerformanceDto> MostBookedServices { get; set; } = [];

        // ── Latest Activity ──────────────────────────────────────────────────
        public List<BookingModel> RecentBookings { get; set; } = [];
        public List<RecentActivityDto> RecentActivities { get; set; } = [];

        // ── Smart Insights ───────────────────────────────────────────────────
        public List<DashboardInsightDto> Insights { get; set; } = [];
    }
}
