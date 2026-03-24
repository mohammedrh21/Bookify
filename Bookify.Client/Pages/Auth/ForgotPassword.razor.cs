using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Auth;

public partial class ForgotPassword : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ForgotPasswordRequestModel _model = new();
    private string _error = string.Empty;
    private bool _loading = false;

    private async Task HandleSubmit()
    {
        _loading = true;
        _error = string.Empty;
        StateHasChanged();

        try
        {
            var result = await AuthService.ForgotPasswordAsync(_model);

            if (result.Success)
            {
                ToastService.ShowSuccess("OTP sent successfully!");
                Nav.NavigateTo($"/verify-otp?email={Uri.EscapeDataString(_model.Email)}");
            }
            else
            {
                _error = result.Message ?? "Failed to send OTP.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
