using Bookify.Client.Models;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Admin.Clients
{
    public partial class AdminClients : ComponentBase, IDisposable
    {
        [Inject] private IUserApiService UserApiService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private List<AdminClientModel>? _clients;
        private bool _loading = true;

        // Pagination
        private int _page = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private int _totalPages => (int)Math.Ceiling((double)_totalCount / _pageSize);

        // Filters
        private string? _search;
        private System.Timers.Timer? _debounceTimer;

        protected override async Task OnInitializedAsync()
        {
            await LoadClientsAsync();
        }

        private async Task LoadClientsAsync()
        {
            _loading = true;
            try
            {
                var result = await UserApiService.GetAdminClientsAsync(_search, _page, _pageSize);
                _clients = result.Items.ToList();
                _totalCount = result.TotalCount;
            }
            finally
            {
                _loading = false;
            }
        }

        private void OnSearchChanged(ChangeEventArgs e)
        {
            _search = e.Value?.ToString();
            
            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Dispose();
            }

            _debounceTimer = new System.Timers.Timer(500);
            _debounceTimer.Elapsed += async (s, ev) =>
            {
                _page = 1;
                await InvokeAsync(LoadClientsAsync);
                await InvokeAsync(StateHasChanged);
            };
            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();
        }

        private async Task ResetFilters()
        {
            _search = string.Empty;
            _page = 1;
            await LoadClientsAsync();
        }

        private async Task OnPageChanged(int newPage)
        {
            _page = newPage;
            await LoadClientsAsync();
        }

        private void NavigateToDetails(Guid clientId)
        {
            Navigation.NavigateTo($"/admin/clients/{clientId}");
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpper();
            return (parts[0][..1] + parts[^1][..1]).ToUpper();
        }

        public void Dispose()
        {
            _debounceTimer?.Dispose();
        }
    }
}
