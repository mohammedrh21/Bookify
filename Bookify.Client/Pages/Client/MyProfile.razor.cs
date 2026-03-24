using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bookify.Client.Models.Profile;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Client;

public partial class MyProfile
{
    [Inject] private IProfileApiService ProfileService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ClientProfileModel? _profile;
    private UpdateClientProfileModel _edit = new();
    private ChangePasswordModel _pwd = new();
    private bool _loading = true;
    private bool _saving = false;
    private bool _changingPwd = false;
    private bool _uploading = false;
    private string _pwdError = string.Empty;
    private string _genderValue = "0";

    private bool _showCurrent;
    private bool _showNew;
    private bool _showConfirm;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ProfileService.GetClientProfileAsync();
            if (result.Success && result.Data is not null)
            {
                _profile = result.Data;
                _edit = new UpdateClientProfileModel
                {
                    FullName    = _profile.FullName,
                    Phone       = _profile.Phone,
                    Gender      = _profile.Gender,
                    DateOfBirth = _profile.DateOfBirth
                };
                _genderValue = (_profile.Gender ?? GenderType.Female).ToString("d");
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to load profile.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading your profile.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SaveProfileAsync()
    {
        if (_profile is null) return;
        _edit.Gender = (GenderType)int.Parse(_genderValue);

        _saving = true;
        try
        {
            var result = await ProfileService.UpdateClientProfileAsync(_edit);
            if (result.Success)
            {
                _profile.FullName = _edit.FullName;
                _profile.Phone    = _edit.Phone;
                _profile.Gender   = _edit.Gender;
                _profile.DateOfBirth = _edit.DateOfBirth;
                ToastService.ShowSuccess("Profile updated successfully.");
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to update profile.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while saving your profile.");
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task ChangePasswordAsync()
    {
        _pwdError = string.Empty;
        if (string.IsNullOrWhiteSpace(_pwd.CurrentPassword) || string.IsNullOrWhiteSpace(_pwd.NewPassword) || string.IsNullOrWhiteSpace(_pwd.ConfirmPassword))
        {
            _pwdError = "All password fields are required."; return;
        }
        if (_pwd.NewPassword.Length < 6)
        {
            _pwdError = "New password must be at least 6 characters."; return;
        }
        if (_pwd.NewPassword != _pwd.ConfirmPassword)
        {
            _pwdError = "New password and confirmation do not match."; return;
        }

        _changingPwd = true;
        try
        {
            var result = await ProfileService.ChangeClientPasswordAsync(_pwd);
            if (result.Success)
            {
                _pwd = new();
                ToastService.ShowSuccess("Password updated successfully.");
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

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        if (file.Size > 2 * 1024 * 1024)
        {
            ToastService.ShowError("File size exceeds 2MB limit.");
            return;
        }

        _uploading = true;
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(2 * 1024 * 1024));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var result = await ProfileService.UploadProfileImageAsync(content, "Client");
            if (result.Success)
            {
                _profile!.ImagePath = result.Data;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
        finally
        {
            _uploading = false;
        }
    }

    private string GetInitial() => _profile?.FullName is { Length: > 0 } name ? name[0].ToString().ToUpper() : "C";
    private static string GetGenderLabel(GenderType g) => g.ToString();
}
