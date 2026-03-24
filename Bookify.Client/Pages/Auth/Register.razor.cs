using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Auth;

public partial class Register : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private RegisterRequest _model = new();
    private bool _loading = false;
    private string _error = string.Empty;
    private bool _showPassword = false;

    private int _step = 1;
    private bool _isStaffRegister = false;

    private void TogglePassword() => _showPassword = !_showPassword;

    private void SelectRole(bool isStaff) => _isStaffRegister = isStaff;

    private void NextStep() => _step = 2;

    private void PreviousStep()
    {
        _step = 1;
        _error = string.Empty;
    }

    private string StepClass(int stepNumber)
    {
        return _step >= stepNumber
            ? "text-blue-600 font-semibold"
            : "text-gray-400";
    }

    private async Task HandleRegister()
    {
        _error = string.Empty;
        _loading = true;
        StateHasChanged();

        try
        {
            var result = _isStaffRegister
                ? await AuthService.RegisterStaffAsync(_model)
                : await AuthService.RegisterClientAsync(_model);

            if (result.Success)
            {
                ToastService.ShowSuccess("Account created successfully! Please login.");
                Nav.NavigateTo("/login");
            }
            else
            {
                _error = result.Message ?? "Registration failed. Please try again.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred during registration.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
