using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;
using Bookify.Client.Models.Booking;

namespace Bookify.Client.Pages.Staff;

public partial class StaffDashboard : ComponentBase
{
    [Inject] private IStatisticsApiService StatsService { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private StaffDashboardResponse _stats = new();
    private bool _loading = true;
    private string _staffName = "Staff Member";

    // ── Period Filter (day / week / month / year) ──────────────────────────
    private string _currentPeriod = "month";
    private DateTime _startDate;
    private DateTime _endDate;

    protected override async Task OnInitializedAsync()
    {
        var name = await LocalStorage.GetItemAsync<string>("user_name");
        if (!string.IsNullOrEmpty(name)) _staffName = name;

        // Default to month
        _currentPeriod = "month";
        CalculateDateRange();
        await LoadDashboardAsync();
    }

    private void SetPeriod(string period)
    {
        if (_currentPeriod == period) return;
        
        _currentPeriod = period;
        CalculateDateRange();
        _ = LoadDashboardAsync();
    }

    private void CalculateDateRange()
    {
        var today = DateTime.Today;
        
        switch (_currentPeriod)
        {
            case "day":
                _startDate = today;
                _endDate = today;
                break;
            case "week":
                // Start from Monday of current week
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                _startDate = today.AddDays(-1 * diff).Date;
                _endDate = _startDate.AddDays(6);
                break;
            case "year":
                _startDate = new DateTime(today.Year, 1, 1);
                _endDate = new DateTime(today.Year, 12, 31);
                break;
            default: // month
                _startDate = new DateTime(today.Year, today.Month, 1);
                _endDate = _startDate.AddMonths(1).AddDays(-1);
                break;
        }
    }

    private async Task LoadDashboardAsync()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            var result = await StatsService.GetStaffDashboardStatsAsync(_startDate, _endDate);
            if (result.Success && result.Data != null)
                _stats = result.Data;
            else { /* errors are already shown by BaseApiService */ }
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private string GetStatusColor(string status) => status switch
    {
        "Approved" => "bg-blue-50 text-blue-700 border border-blue-100",
        "Completed" => "bg-emerald-50 text-emerald-700 border border-emerald-100",
        "Pending" => "bg-amber-50 text-amber-700 border border-amber-100",
        "Cancelled" => "bg-red-50 text-red-700 border border-red-100",
        _ => "bg-gray-50 text-gray-600 border border-gray-200"
    };

    private string PeriodBtnClass(string period) =>
        $"px-4 py-1.5 text-xs font-bold rounded-xl transition-all " +
        (period == _currentPeriod
            ? "bg-white text-primary-600 shadow-sm"
            : "text-gray-500 hover:text-gray-700 hover:bg-gray-100");
}

