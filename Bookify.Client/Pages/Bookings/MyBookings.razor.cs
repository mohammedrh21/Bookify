using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Models.Review;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Bookings;

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

    private List<BookingModel> FilteredBookings => _activeTab == "All"
        ? _bookings
        : _bookings.Where(b => b.Status.Equals(_activeTab, StringComparison.OrdinalIgnoreCase)).ToList();

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
                }
                else
                {
                    ToastService.ShowError(result.Message ?? "Failed to load bookings.");
                }
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading your bookings.");
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
                if (b is not null) b.Status = "Cancelled";
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to cancel booking.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while cancelling the booking.");
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
                CloseRatingModal();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to submit review.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while submitting your review.");
        }
        finally
        {
            _submittingReview = false;
        }
    }
}
