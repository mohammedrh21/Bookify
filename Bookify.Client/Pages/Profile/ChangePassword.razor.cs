using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Profile;
using Bookify.Client.Services;
using Bookify.Client.Models;
using Bookify.Client.Models.Common;

namespace Bookify.Client.Pages.Profile;

public partial class ChangePassword
{
    [Inject] private IProfileApiService ProfileService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private ChangePasswordModel _pwd = new();
    private bool _loading = true;
    private bool _changingPwd = false;
    private string _pwdError = string.Empty;

    private bool _showCurrent;
    private bool _showNew;
    private bool _showConfirm;
    
    private string _userRole = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _userRole = await AuthService.GetUserRoleAsync() ?? string.Empty;
        }
        catch
        {
            ToastService.ShowError("Failed to verify user role.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task ChangePasswordAsync()
    {
        _pwdError = string.Empty;
        if (string.IsNullOrWhiteSpace(_pwd.CurrentPassword) || string.IsNullOrWhiteSpace(_pwd.NewPassword) || string.IsNullOrWhiteSpace(_pwd.ConfirmPassword))
        {
            _pwdError = "All password fields are required."; 
            return;
        }
        if (_pwd.NewPassword.Length < 6)
        {
            _pwdError = "New password must be at least 6 characters."; 
            return;
        }
        if (_pwd.NewPassword != _pwd.ConfirmPassword)
        {
            _pwdError = "New password and confirmation do not match."; 
            return;
        }

        _changingPwd = true;
        try
        {
            ApiResult<bool> result;
            
            if (_userRole == "Admin")
            {
                result = await ProfileService.ChangeAdminPasswordAsync(_pwd);
            }
            else if (_userRole == "Staff")
            {
                result = await ProfileService.ChangeStaffPasswordAsync(_pwd);
            }
            else if (_userRole == "Client")
            {
                result = await ProfileService.ChangeClientPasswordAsync(_pwd);
            }
            else 
            {
                _pwdError = "Unauthorized user role.";
                return;
            }

            if (result.Success)
            {
                _pwd = new();
                ToastService.ShowSuccess("Password updated successfully.");
                GoBack();
            }
            else
            {
                _pwdError = result.Message ?? "Failed to change password.";
            }
        }
        catch (Exception)
        {
            _pwdError = "An unexpected error occurred while changing your password.";
        }
        finally
        {
            _changingPwd = false;
        }
    }

    private void GoBack()
    {
        string profileRoute = _userRole switch
        {
            "Admin"  => "/admin/profile",
            "Staff"  => "/staff/profile",
            "Client" => "/client/profile",
            _        => "/"
        };
        Nav.NavigateTo(profileRoute);
    }
}
