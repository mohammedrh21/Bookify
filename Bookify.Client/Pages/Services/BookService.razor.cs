using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Service;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Services;

public partial class BookService : ComponentBase
{
    [Parameter] public Guid ServiceId { get; set; }

    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private IBookingService BookingService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;
    [Inject] private IPaymentApiService PaymentApiService { get; set; } = default!;

    private ServiceModel? _service;
    private bool _loading = true;
    private bool _booking = false;
    private bool _timeSelected = false;
    private DateTime _selectedDate = DateTime.Today.AddDays(1);
    private TimeSpan _selectedTime;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ServiceService.GetByIdAsync(ServiceId);
            if (result.Success)
            {
                _service = result.Data;
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

    private void OnDateChanged(DateTime d)
    {
        _selectedDate = d;
        _timeSelected = false;
    }

    private async Task HandleProceedToPayment()
    {
        if (_selectedDate.Date < DateTime.Today)
        {
            ToastService.ShowError("You cannot book an appointment in the past.");
            return;
        }

        _booking = true;
        StateHasChanged();

        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var clientIdStr = authState.User.FindFirst("sub")?.Value
                           ?? authState.User.FindFirst("nameid")?.Value;

            if (!Guid.TryParse(clientIdStr, out var clientId))
            {
                ToastService.ShowError("User authentication issue. Please try logging in again.");
                return;
            }

            var request = new CreateBookingRequest
            {
                ClientId = clientId,
                ServiceId = _service!.Id,
                StaffId = _service.StaffId,
                Date = _selectedDate,
                Time = _selectedTime
            };

            var sessionResult = await PaymentApiService.CreateCheckoutSessionAsync(request);

            if (sessionResult.Success && !string.IsNullOrEmpty(sessionResult.Data?.CheckoutUrl))
            {
                Nav.NavigateTo(sessionResult.Data.CheckoutUrl, forceLoad: true); // Hard redirect to Stripe checkout
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
            _booking = false;
            StateHasChanged();
        }
    }
}
