using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ServiceApprovals : ComponentBase
{
    [Inject] private IServiceApprovalApiService ApprovalApiService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<ServiceApprovalRequestModel>? _requests;
    private bool _loading = true;
    private bool _showRejectModal;
    private Guid _rejectId;
    private string _rejectComment = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        _loading = true;
        try
        {
            var result = await ApprovalApiService.GetAllRequestsAsync();
            if (result.Success)
            {
                _requests = result.Data?.Where(r => r.Status == ApprovalStatus.Pending).ToList();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to load approval requests.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading requests.");
        }
        finally
        {
            _loading = false;
        }
    }

    private string GetDiffClass<T>(T current, T proposed)
    {
        return !System.Collections.Generic.EqualityComparer<T>.Default.Equals(current, proposed)
            ? "text-blue-600 font-semibold"
            : "text-gray-600";
    }

    private async Task ApproveAsync(Guid id)
    {
        try
        {
            var result = await ApprovalApiService.ApproveAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service request approved!");
                await LoadRequestsAsync();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to approve service.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An error occurred while approving the request.");
        }
    }

    private void OpenRejectModal(Guid id)
    {
        _rejectId = id;
        _rejectComment = string.Empty;
        _showRejectModal = true;
    }

    private void CancelReject() => _showRejectModal = false;

    private async Task ConfirmReject()
    {
        if (string.IsNullOrWhiteSpace(_rejectComment)) return;

        try
        {
            var result = await ApprovalApiService.RejectAsync(_rejectId, _rejectComment);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service request rejected.");
                _showRejectModal = false;
                await LoadRequestsAsync();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to reject service.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An error occurred while rejecting the request.");
        }
    }

    private string FormatTime(TimeSpan ts) =>
        DateTime.Today.Add(ts).ToString("hh:mm tt");
}
