using Microsoft.AspNetCore.Components;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Bookings;

public partial class PaymentSuccess : ComponentBase
{
    [Inject] private IPaymentApiService PaymentApiService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "session_id")]
    public string? SessionId { get; set; }

    private bool _isProcessing = true;
    private bool _isSuccess = false;

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(SessionId))
        {
            _isProcessing = false;
            return;
        }

        try
        {
            var result = await PaymentApiService.ConfirmCheckoutSessionAsync(SessionId);
            _isSuccess = result.Success;
        }
        catch
        {
            _isSuccess = false;
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
