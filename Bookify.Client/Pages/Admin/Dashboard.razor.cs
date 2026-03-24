using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Bookify.Client.Services;
using Bookify.Client.Models.Common;

namespace Bookify.Client.Pages.Admin;

public partial class Dashboard : ComponentBase
{
    [Inject] private IStatisticsApiService StatsService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private AdminDashboardResponse _stats = new();
    private bool _loading = true;
    private DateTime _startDate = DateTime.Today.AddDays(-30);
    private DateTime _endDate = DateTime.Today;
    private string CurrentRange = "month";
    private bool _chartsRendered = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        _loading = true;
        _chartsRendered = false;  
        StateHasChanged();

        try
        {
            var result = await StatsService.GetAdminDashboardStatsAsync(_startDate, _endDate);

            if (result.Success && result.Data != null)
            {
                _stats = result.Data;
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to load dashboard data.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading the dashboard.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private void SetRange(string range)
    {
        CurrentRange = range;
        _endDate = DateTime.Today;
        _startDate = range switch
        {
            "week" => DateTime.Today.AddDays(-7),
            "month" => DateTime.Today.AddDays(-30),
            "year" => DateTime.Today.AddYears(-1),
            _ => _startDate
        };
        _ = LoadDashboardAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_loading || _chartsRendered) return;

        _chartsRendered = true;

        try
        {
            if (_stats.BookingTrends.Any())
            {
                await JS.InvokeVoidAsync(
                    "chartInterop.renderLineChart",
                    "bookingsChart",
                    _stats.BookingTrends.Select(x => x.Label).ToArray(),
                    _stats.BookingTrends.Select(x => x.Value).ToArray(),
                    "Bookings");
            }

            if (_stats.CategoryDistribution.Any())
            {
                await JS.InvokeVoidAsync(
                    "chartInterop.renderDoughnutChart",
                    "categoryChart",
                    _stats.CategoryDistribution.Select(x => x.Label).ToArray(),
                    _stats.CategoryDistribution.Select(x => x.Value).ToArray());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering charts: {ex.Message}");
        }
    }

    private string GetStatusBadgeClass(string status) => status switch
    {
        "Approved" => "bg-blue-50 text-blue-600",
        "Completed" => "bg-green-50 text-green-600",
        "Pending" => "bg-amber-50 text-amber-600",
        "Cancelled" => "bg-red-50 text-red-600",
        _ => "bg-gray-50 text-gray-500"
    };

    private string GetInsightBorderColor(string color) => color switch {
        "green" => "border-s-green-500",
        "blue" => "border-s-blue-500",
        "yellow" => "border-s-yellow-500",
        _ => "border-s-primary-500"
    };

    private string GetInsightIconBg(string color) => color switch {
        "green" => "bg-green-50 text-green-600",
        "blue" => "bg-blue-50 text-blue-600",
        "yellow" => "bg-yellow-50 text-yellow-600",
        _ => "bg-primary-50 text-primary-600"
    };

    private double CalculateWidth(int count)
    {
        var max = _stats.MostBookedServices.Any() ? _stats.MostBookedServices.Max(x => (double)x.BookingCount) : 0;
        return max > 0 ? (count / max) * 100 : 0;
    }
}
