using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Services;

public partial class ServiceDetails
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private IReviewApiService ReviewService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IBookingService BookingService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ServiceModel? _service;
    private List<ReviewModel>? _reviews;
    private bool _loading = true;

    private bool _canRate = false;
    private bool _rateSubmitted = false;
    private bool _submittingReview = false;
    private CreateReviewRequest _newReview = new() { Rating = 5 };
    private Guid? _eligibleBookingId;

    private string? _userRole;
    private string _backUrl = "/services";
    private string _backLabel = "Back to all services";

    protected override async Task OnInitializedAsync()
    {
        _userRole = await AuthService.GetUserRoleAsync();
        SetBackNavigation();

        try
        {
            var svcTask = ServiceService.GetByIdAsync(Id);
            var revTask = ReviewService.GetServiceReviewsAsync(Id);

            await Task.WhenAll(svcTask, revTask);

            var svcResult = svcTask.Result;
            var revResult = revTask.Result;

            if (svcResult.Success)
            {
                _service = svcResult.Data;
            }
            else
            {
                ToastService.ShowError(svcResult.Message ?? "Failed to load service details.");
            }

            if (revResult.Success)
            {
                _reviews = revResult.Data?.Items.ToList();
            }
            else
            {
                // Optionally suppress toast for review load failure if it's non-critical
                Console.WriteLine(revResult.Message ?? "Failed to load reviews.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading service details.");
        }

        await CheckEligibility();

        _loading = false;
    }

    private void SetBackNavigation()
    {
        switch (_userRole)
        {
            case "Admin":
                _backUrl = "/admin/services";
                _backLabel = "Back to admin services";
                break;
            case "Staff":
                _backUrl = "/services/my-service";
                _backLabel = "Back to my service";
                break;
            default:
                _backUrl = "/services";
                _backLabel = "Back to all services";
                break;
        }
    }

    private async Task CheckEligibility()
    {
        var userId = await AuthService.GetUserIdAsync();
        if (userId == null) return;

        var bookingsResult = await BookingService.GetClientBookingsAsync(userId.Value);
        if (bookingsResult.Success && bookingsResult.Data != null)
        {
            var booking = bookingsResult.Data.FirstOrDefault(b =>
                b.ServiceId == Id &&
                b.Status == "Completed");

            if (booking != null)
            {
                _canRate = true;
                _eligibleBookingId = booking.Id;
                _newReview.BookingId = booking.Id;
            }
        }
    }

    private async Task SubmitReview()
    {
        if (_newReview.Rating < 1 || _newReview.Rating > 5) return;

        _submittingReview = true;
        try
        {
            var result = await ReviewService.CreateReviewAsync(_newReview);

            if (result.Success)
            {
                ToastService.ShowSuccess("Review submitted successfully!");
                _rateSubmitted = true;
                var revResult = await ReviewService.GetServiceReviewsAsync(Id);
                if (revResult.Success)
                    _reviews = revResult.Data?.Items.ToList();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to submit review.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while submitting the review.");
        }
        finally
        {
            _submittingReview = false;
        }
    }

    private void NavigateToBooking() => Nav.NavigateTo($"/book/{Id}");

    private string FormatTime(TimeSpan ts) =>
        DateTime.Today.Add(ts).ToString("hh:mm tt");
}
