using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;
using Blazored.LocalStorage;

namespace Bookify.Client.Pages.Staff;

public partial class MyService
{
    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IServiceApprovalApiService ApprovalApiService { get; set; } = default!;
    [Inject] private IAuthService Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;
    [Inject] private ILocalStorageService localStorage { get; set; } = default!;

    // ── State ───────────────────────────────────────────────────────────────
    private ServiceModel? _service;
    private ServiceApprovalRequestModel? _pendingRequest;
    private bool _hasPendingRequest;
    private ServiceApprovalRequestModel? _rejectedRequest;
    private bool _showRejectedBanner;
    private ServiceModel? _editSnapshot;

    private bool _loading = true;
    private bool _loadError;
    private bool _uploading;
    private bool _isUpdating;
    private bool _isEditing;

    private Guid _staffId;

    private bool _showDeleteModal;
    private int _deleteCountdown = 3;

    // ── Lifecycle ───────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loading = true;
        _loadError = false;

        try
        {
            var staff = await Auth.GetUserIdAsync();
            if (staff == null)
            {
                _loadError = true;
                ToastService.ShowError("User authentication failed.");
                return;
            }
            
            _staffId = staff.Value;

            var result = await ServiceService.GetByStaffIdAsync(_staffId);
            if (result.Success)
            {
                _service = result.Data;
            }

            var approvalsResult = await ApprovalApiService.GetMyRequestsAsync();
            if (approvalsResult.Success)
            {
                var requests = approvalsResult.Data;
                _pendingRequest = requests?.FirstOrDefault(r => r.Status == ApprovalStatus.Pending);
                _hasPendingRequest = _pendingRequest != null;

                if (!_hasPendingRequest)
                {
                    _rejectedRequest = requests?.FirstOrDefault(r => r.Status == ApprovalStatus.Rejected);
                    if (_rejectedRequest != null)
                    {
                        var dismissedStr = await localStorage.GetItemAsync<string>($"dismissed_rejection_{_rejectedRequest.Id}");
                        _showRejectedBanner = string.IsNullOrEmpty(dismissedStr);
                    }
                }
            }
        }
        catch (Exception)
        {
            _loadError = true;
            ToastService.ShowError("Failed to load your service details.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task ReloadAsync() => await LoadDataAsync();

    // ── Navigation ──────────────────────────────────────────────────────────
    private void GoToCreate() => Nav.NavigateTo("/services/create-service");

    // ── Edit Mode ───────────────────────────────────────────────────────────
    private void EnterEditMode()
    {
        _editSnapshot = CloneService(_service!);
        _isEditing = true;
    }

    private void CancelEdit()
    {
        if (_editSnapshot != null)
            _service = _editSnapshot;

        _editSnapshot = null;
        _isEditing = false;
    }

    private async Task HandleUpdateSubmit()
    {
        if (_service == null) return;

        _isUpdating = true;
        try
        {
            var result = await ApprovalApiService.SubmitUpdateAsync(_service);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service update request submitted successfully!");
                _isEditing = false;
                _editSnapshot = null;

                var refresh = await ServiceService.GetByStaffIdAsync(_staffId);
                if (refresh.Success)
                    _service = refresh.Data;

                var approvalsRefresh = await ApprovalApiService.GetMyRequestsAsync();
                if (approvalsRefresh.Success)
                {
                    var requests = approvalsRefresh.Data;
                    _pendingRequest = requests?.FirstOrDefault(r => r.Status == ApprovalStatus.Pending);
                    _hasPendingRequest = _pendingRequest != null;
                }
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to submit update request.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while updating the service.");
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private static ServiceModel CloneService(ServiceModel src) => new()
    {
        Id = src.Id,
        Name = src.Name,
        Description = src.Description,
        Duration = src.Duration,
        TimeStart = src.TimeStart,
        TimeEnd = src.TimeEnd,
        Price = src.Price,
        CategoryName = src.CategoryName,
        CategoryId = src.CategoryId,
        StaffName = src.StaffName,
        StaffId = src.StaffId,
        IsDeleted = src.IsDeleted,
        Rating = src.Rating,
        ReviewCount = src.ReviewCount,
        ImagePath = src.ImagePath,
    };

    // ── Image Upload / Remove ───────────────────────────────────────────────
    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null || _service == null) return;

        if (file.Size > 2 * 1024 * 1024)
        {
            ToastService.ShowError("File size exceeds 2 MB limit.");
            return;
        }

        _uploading = true;
        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(2 * 1024 * 1024));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var result = await ServiceService.UploadServiceImageAsync(_service.Id, content);
            if (result.Success)
            {
                _service.ImagePath = result.Data;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
        finally
        {
            _uploading = false;
        }
    }

    private async Task RemoveImage()
    {
        if (_service == null || string.IsNullOrEmpty(_service.ImagePath)) return;

        _uploading = true;
        try
        {
            var result = await ServiceService.RemoveServiceImageAsync(_service.Id);
            if (result.Success)
            {
                _service.ImagePath = null;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError("Could not remove the image: " + ex.Message);
        }
        finally
        {
            _uploading = false;
        }
    }

    // ── Delete ──────────────────────────────────────────────────────────────
    private async Task OpenDeleteModal()
    {
        if (_showDeleteModal) return;

        _showDeleteModal = true;
        _deleteCountdown = 10;
        StateHasChanged();

        while (_deleteCountdown > 0 && _showDeleteModal)
        {
            await Task.Delay(1000);
            if (!_showDeleteModal) break;
            _deleteCountdown--;
            StateHasChanged();
        }
    }

    private void CloseDeleteModal() => _showDeleteModal = false;

    private async Task ConfirmDelete()
    {
        if (_service == null) return;

        try
        {
            var result = await ServiceService.DeleteAsync(_service.Id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service deleted successfully.");
                _service = null;
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to delete service.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while deleting the service.");
        }
        finally
        {
            _showDeleteModal = false;
        }
    }

    private async Task DismissRejection()
    {
        if (_rejectedRequest != null)
        {
            await localStorage.SetItemAsync($"dismissed_rejection_{_rejectedRequest.Id}", "true");
            _showRejectedBanner = false;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private string FormatTime(TimeSpan ts) =>
        DateTime.Today.Add(ts).ToString("hh:mm tt");

    private int DaysUntilCanRecreate()
    {
        if (_service == null || !_service.IsDeleted || _service.DeletedAt == null) return 0;
        var diff = 14 - (DateTime.UtcNow - _service.DeletedAt.Value).Days;
        return diff < 0 ? 0 : diff;
    }
}
