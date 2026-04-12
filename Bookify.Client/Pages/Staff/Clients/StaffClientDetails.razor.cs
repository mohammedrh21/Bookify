using Bookify.Client.Models;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Staff.Clients
{
    public partial class StaffClientDetails
    {
        [Parameter] public Guid ClientId { get; set; }

        [Inject] private IStaffApiService StaffApiService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private StaffClientDetailsModel? _details;
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadClientDetailsAsync();
        }

        private async Task LoadClientDetailsAsync()
        {
            _loading = true;
            try
            {
                _details = await StaffApiService.GetStaffClientDetailsAsync(ClientId);
            }
            finally
            {
                _loading = false;
            }
        }

        private void NavigateBack()
        {
            Navigation.NavigateTo("/staff/clients");
        }

        private void ContactClient(string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                Navigation.NavigateTo($"mailto:{email}");
            }
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpper();
            return (parts[0][..1] + parts[^1][..1]).ToUpper();
        }

        private string GetPaymentBadgeClass(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800";
            
            return status.ToLower() switch
            {
                "succeeded" => "px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-800",
                "pending" => "px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-800",
                "failed" => "px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800",
                _ => "px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
            };
        }
    }
}
