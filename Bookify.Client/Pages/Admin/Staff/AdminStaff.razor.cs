using Bookify.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bookify.Client.Pages.Admin.Staff
{
    public partial class AdminStaff : ComponentBase, IDisposable
    {
        [Inject] private IUserApiService UserApiService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private List<AdminStaffModel>? _staff;
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
            await LoadStaffAsync();
        }

        private async Task LoadStaffAsync()
        {
            _loading = true;
            try
            {
                var result = await UserApiService.GetAdminStaffAsync(_search, _page, _pageSize);
                _staff = result.Items.ToList();
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

            _debounceTimer?.Stop();
            _debounceTimer?.Dispose();

            _debounceTimer = new System.Timers.Timer(500);
            _debounceTimer.Elapsed += async (_, _) =>
            {
                _page = 1;
                await InvokeAsync(LoadStaffAsync);
                await InvokeAsync(StateHasChanged);
            };
            _debounceTimer.AutoReset = false;
            _debounceTimer.Start();
        }

        private async Task ResetFilters()
        {
            _search = string.Empty;
            _page = 1;
            await LoadStaffAsync();
        }

        private async Task OnPageChanged(int newPage)
        {
            _page = newPage;
            await LoadStaffAsync();
        }

        private void NavigateToDetails(Guid staffId)
        {
            Navigation.NavigateTo($"/admin/staff-members/{staffId}");
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
