using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Profile;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ManageStaff : ComponentBase
{
    [Inject] private IUserApiService UserApiService { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    private List<StaffSummaryModel> _staffList = [];
    private bool _loading = true;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        _loading = true;
        try
        {
            var result = await UserApiService.GetStaffAsync(_currentPage, PageSize);
            _staffList = result.Items.ToList();
            _totalPages = result.TotalPages;
            _totalCount = result.TotalCount;
        }
        catch (Exception)
        {
            Toast.ShowError("Failed to load staff list.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadData();
    }

    private async Task ToggleActive(StaffSummaryModel staff)
    {
        try
        {
            var success = await UserApiService.ToggleUserActiveAsync(staff.Id);
            if (success)
            {
                staff.IsActive = !staff.IsActive;
                Toast.ShowSuccess($"Staff member {(staff.IsActive ? "activated" : "deactivated")} successfully.");
            }
            else
            {
                Toast.ShowError("Failed to update status.");
            }
        }
        catch (Exception)
        {
            Toast.ShowError("An unexpected error occurred while toggling staff status.");
        }
    }
}
