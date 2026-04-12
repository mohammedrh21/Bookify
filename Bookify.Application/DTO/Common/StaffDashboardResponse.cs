using Bookify.Application.DTO.Booking;
using Bookify.Application.DTO.Review;

namespace Bookify.Application.DTO.Common
{
    /// <summary>
    /// Response returned exclusively by the Staff dashboard endpoint.
    /// Contains the authenticated staff member's own KPIs, booking timeline,
    /// and client-review statistics.
    /// </summary>
    public class StaffDashboardResponse
    {
        // ── KPIs ────────────────────────────────────────────────────────────
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ApprovedBookings { get; set; }
        public double TotalRevenue { get; set; }
        public double CompletionRate { get; set; }
        public int ActiveClients { get; set; }
        public double CancellationRate { get; set; }

        // ── Trends (% change vs previous period) ────────────────────────────
        public double TotalBookingsTrend { get; set; }
        public double TotalRevenueTrend { get; set; }

        // ── Charts ──────────────────────────────────────────────────────────
        public List<ChartDataPoint> BookingTrends { get; set; } = [];

        // ── Latest Activity ──────────────────────────────────────────────────
        public List<BookingResponse> RecentBookings { get; set; } = [];
        public List<RecentActivityDto> RecentActivities { get; set; } = [];

        // ── Review Statistics ────────────────────────────────────────────────
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        /// <summary>Index 0 = 1-star count, index 4 = 5-star count.</summary>
        public List<int> RatingDistribution { get; set; } = [0, 0, 0, 0, 0];
        public List<ReviewDto> RecentReviews { get; set; } = [];
    }
}
