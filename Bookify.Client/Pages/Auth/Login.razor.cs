using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

    private LoginRequest _model = new();
    private string _error = string.Empty;
    private bool _loading = false;
    private bool _showPassword = false;

    private void TogglePassword() => _showPassword = !_showPassword;

    private async Task HandleLogin()
    {
        _loading = true;
        _error = string.Empty;
        StateHasChanged();

        try
        {
            var result = await AuthService.LoginAsync(_model);

            if (result.Success)
            {
                var role = await AuthService.GetUserRoleAsync();
                ToastService.ShowSuccess("Logged in successfully!");
                
                if (role == "Admin")
                {
                    Nav.NavigateTo("/admin/dashboard", forceLoad: true);
                }
                else if (role == "Staff")
                {
                    Nav.NavigateTo("/staff/dashboard", forceLoad: true);
                }
                else
                {
                    Nav.NavigateTo(ReturnUrl ?? "/", forceLoad: true);
                }
            }
            else
            {
                _error = result.Message ?? "Login failed. Please check your credentials.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred during login.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
