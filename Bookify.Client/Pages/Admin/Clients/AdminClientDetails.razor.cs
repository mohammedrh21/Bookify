using Bookify.Client.Models;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Admin.Clients
{
    public partial class AdminClientDetails : ComponentBase
    {
        [Parameter] public Guid ClientId { get; set; }

        [Inject] private IUserApiService UserApiService { get; set; } = default!;
        [Inject] private ToastService Toast { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private AdminClientDetailsModel? _details;
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
                _details = await UserApiService.GetAdminClientDetailsAsync(ClientId);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task ToggleActive()
        {
            if (_details == null) return;
            try
            {
                var success = await UserApiService.ToggleUserActiveAsync(_details.ClientId);
                if (success)
                {
                    _details.IsActive = !_details.IsActive;
                    Toast.ShowSuccess("Client status updated.");
                }
                else
                {
                    // errors are already shown by BaseApiService
                }
            }
            catch (Exception)
            {
                // unexpected error
            }
        }

        private void NavigateBack()
        {
            Navigation.NavigateTo("/admin/clients");
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
