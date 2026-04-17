using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Auth;
using Bookify.Client.Models.Common;
using Bookify.Client.Data;
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
    private string _otp = string.Empty;

    private int _step = 1;
    private bool _isStaffRegister = false;

    // Country Picker State
    private CountryModel _selectedCountry = CountryData.Countries.FirstOrDefault(c => c.Iso3Code == "SAU") ?? CountryData.Countries.First();
    private string _phoneNumber
    {
        get => _internalPhoneNumber;
        set
        {
            _internalPhoneNumber = value;
            UpdateModelPhone();
        }
    }
    private string _internalPhoneNumber = string.Empty;

    private void UpdateModelPhone()
    {
        if (!string.IsNullOrWhiteSpace(_internalPhoneNumber))
        {
            var cleanPhone = _internalPhoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanPhone.StartsWith("0"))
            {
                cleanPhone = cleanPhone[1..];
            }
            _model.Phone = $"{_selectedCountry.DialCode}{cleanPhone}";
        }
        else
        {
            _model.Phone = string.Empty;
        }
    }

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

    private void HandleCountrySelected(CountryModel country)
    {
        _selectedCountry = country;
        UpdateModelPhone();
        StateHasChanged();
    }

    /// <summary>
    /// Step 2 submit: Initiates registration by sending OTP to the user's email.
    /// No database record is created yet.
    /// </summary>
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
                _otp = string.Empty;
                _step = 3;
                ToastService.ShowSuccess(result.Message ?? "OTP sent! Check your email.");
            }
            else
            {
                _error = result.Message ?? "Registration failed. Please try again.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred. Please try again.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Step 3 submit: Verifies the OTP. If correct, the account is saved to the database.
    /// If wrong, nothing is committed.
    /// </summary>
    private async Task HandleVerifyOtp()
    {
        _error = string.Empty;
        _loading = true;
        StateHasChanged();

        try
        {
            var result = await AuthService.VerifyRegistrationOtpAsync(new VerifyRegistrationOtpRequestModel
            {
                Email = _model.Email,
                Otp   = _otp
            });

            if (result.Success)
            {
                ToastService.ShowSuccess("Account created successfully! Please login.");
                Nav.NavigateTo("/login");
            }
            else
            {
                _error = result.Message ?? "Invalid OTP. Please try again.";
                ToastService.ShowError(_error);
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred during verification.");
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
