using Bookify.Client.Models.Profile;
using Bookify.Client.Models.Common;
using Bookify.Client.Data;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Admin
{
    public partial class AdminProfile : ComponentBase
    {
        [Inject]
        public IProfileApiService ProfileService { get; set; } = default!;

        [Inject]
        public ToastService ToastService { get; set; } = default!;

        private AdminProfileModel? _profile;
        private UpdateAdminProfileModel _edit = new();
        private bool _loading = true;
        private bool _saving = false;

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
            var result = await ProfileService.GetAdminProfileAsync();
            if (result.Success && result.Data is not null)
            {
                _profile = result.Data;
                _edit = new UpdateAdminProfileModel
                {
                    FullName = _profile.FullName,
                    Phone = _profile.Phone
                };
                ExtractPhoneData(_profile.Phone);
            }
            // errors are already shown by BaseApiService
            _loading = false;
        }

        private async Task SaveProfileAsync()
        {
            if (_profile is null) return;

            if (!string.IsNullOrWhiteSpace(_phoneNumber))
            {
                var cleanPhone = _phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (cleanPhone.StartsWith("0"))
                    cleanPhone = cleanPhone[1..];
                _edit.Phone = $"{_selectedCountry.DialCode}{cleanPhone}";
            }
            else
            {
                _edit.Phone = string.Empty;
            }

            _saving = true;
            var result = await ProfileService.UpdateAdminProfileAsync(_edit);
            if (result.Success)
            {
                _profile.FullName = _edit.FullName;
                _profile.Phone = _edit.Phone;
                ToastService.ShowSuccess("Admin profile updated successfully!");
            }
            // errors are already shown by BaseApiService
            _saving = false;
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

        private string GetInitial() => _profile?.FullName is { Length: > 0 } name ? name[0].ToString().ToUpper() : "A";
    }
}
