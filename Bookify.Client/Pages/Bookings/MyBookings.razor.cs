using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Models.Review;
using Bookify.Client.Services;
using System.Timers;

namespace Bookify.Client.Pages.Bookings;

public enum BookingSortOrder
{
    NewestFirst,
    OldestFirst
}

public partial class MyBookings : ComponentBase
{
    [Inject] private IBookingService BookingService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IReviewApiService ReviewService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<BookingModel> _bookings = [];
    private bool _loading = true;
    private string _activeTab = "All";

    private readonly string[] _tabs = ["All", "Pending", "Approved", "Completed", "Cancelled"];

    private Guid? _bookingToCancel;
    private bool _cancelModalVisible = false;

    // Rating Props
    private BookingModel? _bookingToRate;
    private bool _ratingModalVisible = false;
    private int _rating = 0;
    private string _reviewComment = "";
    private bool _submittingReview = false;

    // Search, Sort, and Pagination Props
    private string _searchQuery = "";
    private BookingSortOrder _sortOrder = BookingSortOrder.NewestFirst;
    private int _currentPage = 1;
    private const int PageSize = 6;
    private System.Timers.Timer? _searchDebounceTimer;

    private List<BookingModel> _paginatedBookings = [];
    private int _totalItems = 0;
    private int _totalPages = 1;

    private void ApplyFilters()
    {
        var query = _bookings.AsEnumerable();

        if (_activeTab != "All")
            query = query.Where(b => b.Status.Equals(_activeTab, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(_searchQuery))
            query = query.Where(b => b.ServiceName != null && b.ServiceName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

        if (_sortOrder == BookingSortOrder.OldestFirst)
            query = query.OrderBy(b => b.Date);
        else
            query = query.OrderByDescending(b => b.Date);

        _totalItems = query.Count();
        _totalPages = Math.Max(1, (int)Math.Ceiling(_totalItems / (double)PageSize));

        if (_currentPage > _totalPages)
            _currentPage = Math.Max(1, _totalPages);

        _paginatedBookings = query
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    private void SetTab(string tab)
    {
        _activeTab = tab;
        _currentPage = 1;
        ApplyFilters();
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? "";
        _currentPage = 1;
        
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();
        
        _searchDebounceTimer = new System.Timers.Timer(300);
        _searchDebounceTimer.Elapsed += (s, ev) => 
        {
            InvokeAsync(() => 
            {
                ApplyFilters();
                StateHasChanged();
            });
        };
        _searchDebounceTimer.AutoReset = false;
        _searchDebounceTimer.Start();
    }

    private void SetSortOrder(ChangeEventArgs e)
    {
        if (Enum.TryParse<BookingSortOrder>(e.Value?.ToString(), out var result))
        {
            _sortOrder = result;
            _currentPage = 1;
            ApplyFilters();
        }
    }
    
    private void NextPage()
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            ApplyFilters();
        }
    }

    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            ApplyFilters();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var userId = await AuthService.GetUserIdAsync();
            if (userId.HasValue)
            {
                var result = await BookingService.GetClientBookingsAsync(userId.Value);
                if (result.Success)
                {
                    _bookings = result.Data ?? [];
                    ApplyFilters();
                }
                else
                {
                    // errors are already shown by BaseApiService
                }
            }
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowCancelModal(Guid bookingId)
    {
        _bookingToCancel = bookingId;
        _cancelModalVisible = true;
    }

    private void CloseCancelModal()
    {
        _bookingToCancel = null;
        _cancelModalVisible = false;
    }

    private async Task ConfirmCancel()
    {
        if (_bookingToCancel.HasValue)
        {
            await CancelBooking(_bookingToCancel.Value);
        }
        CloseCancelModal();
    }

    private async Task CancelBooking(Guid id)
    {
        try
        {
            var userId = await AuthService.GetUserIdAsync();
            if (userId is null) return;

            var result = await BookingService.CancelAsync(id, userId.Value, "Client");
            if (result.Success)
            {
                ToastService.ShowSuccess("Booking cancelled.");
                var b = _bookings.FirstOrDefault(x => x.Id == id);
                if (b is not null) 
                {
                    b.Status = "Cancelled";
                    ApplyFilters();
                }
            }
            else
            {
                // errors are already shown by BaseApiService
            }
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            StateHasChanged();
        }
    }

    // Rating Methods
    private void ShowRatingModal(BookingModel booking)
    {
        _bookingToRate = booking;
        _rating = 0;
        _reviewComment = "";
        _ratingModalVisible = true;
    }

    private void CloseRatingModal()
    {
        _bookingToRate = null;
        _ratingModalVisible = false;
    }

    private async Task SubmitReview()
    {
        if (_bookingToRate == null || _rating == 0) return;

        _submittingReview = true;
        try
        {
            var request = new CreateReviewRequest
            {
                BookingId = _bookingToRate.Id,
                Rating = _rating,
                Comment = _reviewComment
            };

            var result = await ReviewService.CreateReviewAsync(request);

            if (result.Success)
            {
                ToastService.ShowSuccess("Thank you for your review!");
                _bookingToRate.IsReviewed = true;
                CloseRatingModal();
            }
            else
            {
                // errors are already shown by BaseApiService
            }
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            _submittingReview = false;
        }
    }

    public void Dispose()
    {
        _searchDebounceTimer?.Dispose();
    }
}
