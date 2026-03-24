using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Auth;

public partial class ResetPassword : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    [SupplyParameterFromQuery] public string Email { get; set; } = string.Empty;
    [SupplyParameterFromQuery] public string Token { get; set; } = string.Empty;

    private ResetPasswordRequestModel _model = new();
    private string _error = string.Empty;
    private bool _loading = false;
    private bool _showPassword = false;

    protected override void OnInitialized()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Token))
        {
            Nav.NavigateTo("/forgot-password");
        }
        _model.Email = Email;
        _model.ResetToken = Token;
    }

    private void TogglePassword() => _showPassword = !_showPassword;

    private async Task HandleSubmit()
    {
        _loading = true;
        _error = string.Empty;
        StateHasChanged();

        try
        {
            var result = await AuthService.ResetPasswordAsync(_model);

            if (result.Success)
            {
                ToastService.ShowSuccess("Password reset successfully!");
                Nav.NavigateTo("/login");
            }
            else
            {
                _error = result.Message ?? "Reset failed.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while resetting the password.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
