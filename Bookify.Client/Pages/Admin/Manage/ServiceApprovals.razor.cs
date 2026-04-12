using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin.Manage;

public partial class ServiceApprovals : ComponentBase
{
    [Inject] private IServiceApprovalApiService ApprovalApiService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<ServiceApprovalRequestModel>? _requests;
    private ServiceApprovalRequestModel? _selectedRequest;
    private bool _loading = true;
    private bool _isProcessing = false;
    private bool _showRejectModal;
    private Guid _rejectId;
    private string _rejectComment = string.Empty;

    // Search & Pagination
    private string _searchText = string.Empty;
    private int _currentPage = 1;
    private int _pageSize = 8;

    private IEnumerable<ServiceApprovalRequestModel> FilteredRequests => 
        (_requests ?? [])
        .Where(r => r.Status == ApprovalStatus.Pending)
        .Where(r => string.IsNullOrWhiteSpace(_searchText) || 
                   r.StaffName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) || 
                   r.ProposedDetails.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

    private List<ServiceApprovalRequestModel> PagedRequests => 
        FilteredRequests
        .Skip((_currentPage - 1) * _pageSize)
        .Take(_pageSize)
        .ToList();

    private int TotalPages => (int)Math.Ceiling(FilteredRequests.Count() / (double)_pageSize);
    private int TotalCount => FilteredRequests.Count();

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
                _requests = result.Data?.ToList();
                // Select first request if none selected or selection no longer exists
                if (_selectedRequest == null || !(_requests?.Any(r => r.Id == _selectedRequest.Id && r.Status == ApprovalStatus.Pending) ?? false))
                {
                    _selectedRequest = FilteredRequests.FirstOrDefault();
                }
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
        finally
        {
            _loading = false;
        }
    }

    private void SelectRequest(ServiceApprovalRequestModel request)
    {
        _selectedRequest = request;
    }

    private async Task HandlePageChanged(int page)
    {
        _currentPage = page;
        // Optional: select first item on new page
        // _selectedRequest = PagedRequests.FirstOrDefault();
        await Task.CompletedTask;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1; // Reset to first page on search
        _selectedRequest = FilteredRequests.FirstOrDefault();
    }

    private string GetDiffClass<T>(T current, T proposed)
    {
        return !System.Collections.Generic.EqualityComparer<T>.Default.Equals(current, proposed)
            ? "text-blue-600 font-semibold"
            : "text-gray-600";
    }

    private async Task ApproveAsync(Guid id)
    {
        if (_isProcessing) return;
        _isProcessing = true;
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
                // errors are already shown by BaseApiService
                _isProcessing = false;
            }
        }
        catch (Exception)
        {
            // unexpected error
            _isProcessing = false;
        }
        finally
        {
            _isProcessing = false;
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
        if (string.IsNullOrWhiteSpace(_rejectComment) || _isProcessing) return;

        _isProcessing = true;
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
                // errors are already shown by BaseApiService
                _isProcessing = false;
            }
        }
        catch (Exception)
        {
            // unexpected error
            _isProcessing = false;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private string FormatTime(TimeSpan ts) =>
        DateTime.Today.Add(ts).ToString("hh:mm tt");
}
