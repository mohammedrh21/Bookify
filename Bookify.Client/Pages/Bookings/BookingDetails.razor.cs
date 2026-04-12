using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Bookings;

public partial class BookingDetails
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IBookingService BookingService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IReviewApiService ReviewService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private BookingModel? _booking;
    private bool _loading = true;

    // Navigation
    private string _backUrl = "/my-bookings";
    private string _backLabel = "Back to My Bookings";

    // Rating Props
    private bool _ratingModalVisible = false;
    private bool _submittingReview = false;
    private bool _hasReviewed = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var role = await AuthService.GetUserRoleAsync();
            switch (role)
            {
                case "Admin":
                    _backUrl = "/admin/bookings";
                    _backLabel = "Back to Manage Bookings";
                    break;
                case "Staff":
                    _backUrl = "/booking/service-bookings";
                    _backLabel = "Back to Service Bookings";
                    break;
                default:
                    _backUrl = "/my-bookings";
                    _backLabel = "Back to My Bookings";
                    break;
            }

            var result = await BookingService.GetByIdAsync(Id);
            if (result.Success)
            {
                _booking = result.Data;
                await CheckExistingReview();
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
            _loading = false;
        }
    }

    private async Task CheckExistingReview()
    {
        if (_booking == null) return;
        try
        {
            var reviewsResult = await ReviewService.GetServiceReviewsAsync(_booking.ServiceId);
            if (reviewsResult.Success && reviewsResult.Data != null)
            {
                _hasReviewed = reviewsResult.Data.Items.Any(r => r.BookingId == Id);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to check existing reviews.");
        }
    }

    private void OpenRatingModal()
    {
        _ratingModalVisible = true;
    }

    private void CloseRatingModal()
    {
        _ratingModalVisible = false;
    }

    private async Task HandleSubmitReview((int Rating, string Comment) data)
    {
        if (_booking == null || data.Rating == 0) return;

        _submittingReview = true;
        try
        {
            var request = new CreateReviewRequest
            {
                BookingId = Id,
                Rating = data.Rating,
                Comment = data.Comment
            };

            var result = await ReviewService.CreateReviewAsync(request);

            if (result.Success)
            {
                ToastService.ShowSuccess("Thank you for your feedback!");
                _hasReviewed = true;
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
}
