using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Staff;

public partial class StaffDashboard : ComponentBase
{
    [Inject] private IStatisticsApiService StatsService { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private Bookify.Client.Models.Common.StaffDashboardResponse _stats = new();
    private bool _loading = true;
    private string _staffName = "Staff Member";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var name = await LocalStorage.GetItemAsync<string>("user_name");
            if (!string.IsNullOrEmpty(name))
            {
                _staffName = name;
            }

            var result = await StatsService.GetStaffDashboardStatsAsync();

            if (result.Success && result.Data != null)
            {
                _stats = result.Data;
            }
            else
            {
                // Typically silently fail or show subtle error if dashboard stats fail
                ToastService.ShowError(result.Message ?? "Failed to load dashboard statistics.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading the dashboard.");
        }
        finally
        {
            _loading = false;
        }
    }

    private string GetStatusColor(string status) => status switch
    {
        "Approved" => "bg-blue-50 text-blue-700 border border-blue-100",
        "Completed" => "bg-green-50 text-green-700 border border-green-100",
        "Pending" => "bg-amber-50 text-amber-700 border border-amber-100",
        "Cancelled" => "bg-red-50 text-red-700 border border-red-100",
        _ => "bg-gray-50 text-gray-600 border border-gray-200"
    };
}
