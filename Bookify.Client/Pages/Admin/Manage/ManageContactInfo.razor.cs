using Bookify.Client.Models;
using Bookify.Client.Models.ContactInfo;
using Bookify.Client.Models.Common;
using Bookify.Client.Data;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Admin.Manage
{
    public partial class ManageContactInfo : ComponentBase
    {
        [Inject]
        public IContactInfoApiService ContactInfoService { get; set; } = default!;

        private ContactInfoModel _model = new();
        private bool _loading = true;
        private bool _saving = false;
        private bool _isNew = false;
        private int _currentStep = 1;

        private CountryModel _selectedCountry = CountryData.Countries.FirstOrDefault(c => c.Iso3Code == "SAU") ?? CountryData.Countries.First();
        private string _phoneNumberInput = string.Empty;

        private void HandleCountrySelected(CountryModel country)
        {
            _selectedCountry = country;
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            var result = await ContactInfoService.GetAsync();
            if (result.Success && result.Data != null)
            {
                _model = result.Data;
                _isNew = false;

                if (!string.IsNullOrEmpty(_model.PhoneNumber))
                {
                    var matchedCountry = CountryData.Countries
                        .Where(c => _model.PhoneNumber.StartsWith(c.DialCode))
                        .OrderByDescending(c => c.DialCode.Length)
                        .FirstOrDefault();

                    if (matchedCountry != null)
                    {
                        _selectedCountry = matchedCountry;
                        _phoneNumberInput = _model.PhoneNumber.Substring(matchedCountry.DialCode.Length);
                    }
                    else
                    {
                        _phoneNumberInput = _model.PhoneNumber;
                    }
                }
            }
            else
            {
                _isNew = true;
                _model = new ContactInfoModel
                {
                    CallDayFrom = DayOfWeek.Monday,
                    CallDayTo = DayOfWeek.Friday,
                    CallHourFrom = new TimeSpan(9, 0, 0),
                    CallHourTo = new TimeSpan(17, 0, 0)
                };
            }
            _loading = false;
        }

        private string GetStepLabel(int step) => step switch
        {
            1 => "Location",
            2 => "Contact",
            3 => "Availability",
            _ => ""
        };

        private void NextStep()
        {
            if (_currentStep < 3) _currentStep++;
        }

        private void PreviousStep()
        {
            if (_currentStep > 1) _currentStep--;
        }

        private async Task SaveAsync()
        {
            _saving = true;

            if (!string.IsNullOrWhiteSpace(_phoneNumberInput))
            {
                var cleanPhone = _phoneNumberInput.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                if (cleanPhone.StartsWith("0"))
                {
                    cleanPhone = cleanPhone[1..];
                }
                _model.PhoneNumber = $"{_selectedCountry.DialCode}{cleanPhone}";
            }

            ApiResult<Guid> result;
            if (_isNew)
            {
                result = await ContactInfoService.CreateAsync(_model);
            }
            else
            {
                result = await ContactInfoService.UpdateAsync(_model);
            }

            if (result.Success)
            {
                _isNew = false;
                _currentStep = 1; // Reset to start or stay on last? Usually reset or redirect.
            }

            _saving = false;
        }
    }
}
