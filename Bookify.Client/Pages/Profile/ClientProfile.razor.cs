using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bookify.Client.Models.Profile;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;
using Bookify.Client.Data;

namespace Bookify.Client.Pages.Profile;

public partial class ClientProfile
{
    [Inject] private IProfileApiService ProfileService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ClientProfileModel? _profile;
    private UpdateClientProfileModel _edit = new();
    private bool _loading = true;
    private bool _saving = false;
    private bool _uploading = false;
    private string _genderValue = "0";

    // Country Picker State
    private CountryModel _selectedCountry = CountryData.Countries.FirstOrDefault(c => c.Iso3Code == "SAU") ?? CountryData.Countries.First();
    private string _phoneNumber = string.Empty;

    private void HandleCountrySelected(CountryModel country)
    {
        _selectedCountry = country;
        StateHasChanged();
    }

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
                ExtractPhoneData(_profile.Phone);
            }
            else
            {
                // errors are already shown by BaseApiService
            }
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

        // Combine Dial Code and Phone Number
        if (!string.IsNullOrWhiteSpace(_phoneNumber))
        {
            var cleanPhone = _phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (cleanPhone.StartsWith("0"))
            {
                cleanPhone = cleanPhone[1..];
            }
            _edit.Phone = $"{_selectedCountry.DialCode}{cleanPhone}";
        }
        else 
        {
            _edit.Phone = string.Empty;
        }

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
                // errors are already shown by BaseApiService
            }
        }
        catch (Exception)
        {
            // unexpected network/parse error
        }
        finally
        {
            _saving = false;
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

    private void ExtractPhoneData(string? fullPhone)
    {
        if (string.IsNullOrWhiteSpace(fullPhone))
        {
            _phoneNumber = string.Empty;
            return;
        }

        var matchedCountry = CountryData.Countries
            .Where(c => fullPhone.StartsWith(c.DialCode))
            .OrderByDescending(c => c.DialCode.Length)
            .FirstOrDefault();

        if (matchedCountry != null)
        {
            _selectedCountry = matchedCountry;
            _phoneNumber = fullPhone.Substring(matchedCountry.DialCode.Length);
        }
        else
        {
            _phoneNumber = fullPhone;
        }
    }

    private string GetInitial() => _profile?.FullName is { Length: > 0 } name ? name[0].ToString().ToUpper() : "C";
    private static string GetGenderLabel(GenderType g) => g.ToString();
}
