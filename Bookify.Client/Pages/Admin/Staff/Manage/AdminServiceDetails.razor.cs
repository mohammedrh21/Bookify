using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Booking;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;
using Microsoft.AspNetCore.Authorization;

namespace Bookify.Client.Pages.Admin.Staff.Manage;

[Authorize(Roles = "Admin")]
public partial class AdminServiceDetails
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private IReviewApiService ReviewService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private ServiceModel? _service;
    private List<ReviewModel>? _reviews;
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var svcTask = ServiceService.GetByIdAsync(Id);
            var revTask = ReviewService.GetServiceReviewsAsync(Id);

            await Task.WhenAll(svcTask, revTask);

            var svcResult = svcTask.Result;
            var revResult = revTask.Result;

            if (svcResult.Success)
            {
                _service = svcResult.Data;
            }

            if (revResult.Success)
            {
                _reviews = revResult.Data?.Items.ToList();
            }
        }
        catch (Exception)
        {
            // unexpected error
        }

        _loading = false;
    }

    private string FormatTime(TimeSpan ts) =>
        DateTime.Today.Add(ts).ToString("hh:mm tt");
        
    private void NavigateBack()
    {
        if (_service != null && _service.StaffId != Guid.Empty)
        {
            Nav.NavigateTo($"/admin/staff-members/{_service.StaffId}");
        }
        else 
        {
            Nav.NavigateTo("/admin/services");
        }
    }
}
