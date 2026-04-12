using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Staff;

public partial class BookingHistory : ComponentBase
{
    [Inject] private IBookingService BookingService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    // ── Data ──────────────────────────────────────────────────────
    private List<BookingModel>? _bookings;
    private bool _loading = true;

    // ── Pagination ────────────────────────────────────────────────
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private const int PageSize = 10;

    // ── Filters ───────────────────────────────────────────────────
    private string _search = string.Empty;
    private string _statusFilter = "";
    private string _datePreset = "";
    private bool _sortAscending = true;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;

    // ── Debounce timer ────────────────────────────────────────────
    private System.Threading.Timer? _debounceTimer;

    protected override async Task OnInitializedAsync()
    {
        ApplyDatePreset();          // sets _dateFrom / _dateTo for default "Today"
        await LoadBookingsAsync();
    }

    private async Task LoadBookingsAsync()
    {
        _loading = true;
        try
        {
            var staffId = await AuthService.GetUserIdAsync();
            if (staffId.HasValue)
            {
                var result = await BookingService.GetStaffBookingsPagedAsync(
                    staffId.Value,
                    page: _currentPage,
                    pageSize: PageSize,
                    status: string.IsNullOrWhiteSpace(_statusFilter) ? null : _statusFilter,
                    from: _dateFrom,
                    to: _dateTo,
                    search: string.IsNullOrWhiteSpace(_search) ? null : _search,
                    sortAsc: _sortAscending);

                if (result.Success && result.Data != null)
                {
                    _bookings = result.Data.Items.ToList();
                    _totalPages = result.Data.TotalPages;
                    _totalCount = result.Data.TotalCount;
                }
                else
                {
                    // errors are already shown by BaseApiService
                    _bookings = [];
                    _totalPages = 1;
                    _totalCount = 0;
                }
            }
        }
        catch (Exception)
        {
            // unexpected error
            _bookings = [];
            _totalPages = 1;
            _totalCount = 0;
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadBookingsAsync();
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? string.Empty;
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Threading.Timer(async _ =>
        {
            _currentPage = 1;
            await InvokeAsync(async () =>
            {
                await LoadBookingsAsync();
                StateHasChanged();
            });
        }, null, 400, System.Threading.Timeout.Infinite);
    }

    private async Task ApplyFilters()
    {
        _currentPage = 1;
        await LoadBookingsAsync();
    }

    private async Task OnDatePresetChanged()
    {
        ApplyDatePreset();
        _currentPage = 1;
        await LoadBookingsAsync();
    }

    private void ApplyDatePreset()
    {
        var today = DateTime.Today;
        switch (_datePreset)
        {
            case "Today":
                _dateFrom = today;
                _dateTo = today;
                break;
            case "ThisWeek":
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                _dateFrom = today.AddDays(-diff);
                _dateTo = _dateFrom.Value.AddDays(6);
                break;
            case "ThisMonth":
                _dateFrom = new DateTime(today.Year, today.Month, 1);
                _dateTo = _dateFrom.Value.AddMonths(1).AddDays(-1);
                break;
            case "Custom":
                break;
            default: // "All Dates"
                _dateFrom = null;
                _dateTo = null;
                break;
        }
    }

    private async Task ToggleSort()
    {
        _sortAscending = !_sortAscending;
        _currentPage = 1;
        await LoadBookingsAsync();
    }

    private async Task ResetFilters()
    {
        _search = string.Empty;
        _statusFilter = "Pending";
        _datePreset = "Today";
        _sortAscending = true;
        _dateFrom = null;
        _dateTo = null;
        ApplyDatePreset();
        _currentPage = 1;
        await LoadBookingsAsync();
    }

    private async Task Approve(Guid id)
    {
        try
        {
            var result = await BookingService.ConfirmAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Booking approved.");
                await LoadBookingsAsync();
            }
            else { /* errors are already shown by BaseApiService */ }
        }
        catch (Exception) { /* unexpected error */ }
    }

    private async Task Reject(Guid id)
    {
        try
        {
            var result = await BookingService.CancelAsync(id, (await AuthService.GetUserIdAsync()) ?? Guid.Empty, "Staff");
            if (result.Success)
            {
                ToastService.ShowSuccess("Booking rejected.");
                await LoadBookingsAsync();
            }
            else { /* errors are already shown by BaseApiService */ }
        }
        catch (Exception) { /* unexpected error */ }
    }

    private async Task Complete(Guid id)
    {
        try
        {
            var result = await BookingService.CompleteAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Booking marked as completed.");
                await LoadBookingsAsync();
            }
            else { /* errors are already shown by BaseApiService */ }
        }
        catch (Exception) { /* unexpected error */ }
    }


    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
