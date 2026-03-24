using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Auth;

public partial class VerifyOtp : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    [SupplyParameterFromQuery] public string Email { get; set; } = string.Empty;

    private VerifyOtpRequestModel _model = new();
    private string _error = string.Empty;
    private bool _loading = false;

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(Email))
        {
            Nav.NavigateTo("/forgot-password");
        }
        _model.Email = Email;
    }

    private async Task HandleSubmit()
    {
        _loading = true;
        _error = string.Empty;
        StateHasChanged();

        try
        {
            var result = await AuthService.VerifyOtpAsync(_model);

            if (result.Success)
            {
                ToastService.ShowSuccess("OTP verified successfully!");
                var token = result.Data; 
                Nav.NavigateTo($"/reset-password?email={Uri.EscapeDataString(Email)}&token={Uri.EscapeDataString(token ?? string.Empty)}");
            }
            else
            {
                _error = result.Message ?? "Verification failed.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred during OTP verification.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
