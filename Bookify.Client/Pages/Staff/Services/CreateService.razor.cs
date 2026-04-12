using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Category;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Staff.Services;

public partial class CreateService : ComponentBase
{
    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private IServiceApprovalApiService ApprovalApiService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IAuthService Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ServiceModel _model = new();
    private List<CategoryModel> _categories = new();
    private Guid _staffId;
    private bool _isProcessing = false;
    private int _currentStep = 1;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var staff = await Auth.GetUserIdAsync();
            if (staff != null)
                _staffId = (Guid)staff;

            var result = await CategoryService.GetAllAsync();
            if (result.Success)
            {
                _categories = (result.Data ?? [])
                                .Where(x => x.IsActive)
                                .ToList();
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

    private void NextStep()
    {
        _currentStep = 2;
    }

    private void PreviousStep()
    {
        _currentStep = 1;
    }

    private async Task HandleValidSubmit()
    {
        if (_currentStep == 1)
        {
            NextStep();
            return;
        }

        _isProcessing = true;
        _model.StaffId = _staffId;
        try
        {
            var result = await ApprovalApiService.SubmitCreateAsync(_model);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service creation request submitted successfully!");
                Nav.NavigateTo("/services/my-service");
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
            _isProcessing = false;
        }
    }
}
