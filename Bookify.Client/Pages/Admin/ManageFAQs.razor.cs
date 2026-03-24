using Microsoft.AspNetCore.Components;
using Bookify.Client.Models;
using Bookify.Client.Models.FAQ;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ManageFAQs : ComponentBase
{
    [Inject] private IFAQApiService FAQService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private PagedResult<FaqModel> _pagedResult = new();
    private FaqModel _currentFaq = new();
    
    private bool _loading = true;
    private bool _showForm = false;
    private bool _saving = false;
    private bool _isEditing = false;
    private int _currentPage = 1;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadFaqsAsync();
    }

    private async Task LoadFaqsAsync()
    {
        _loading = true;
        try
        {
            var result = await FAQService.GetAllAsync(_currentPage, PageSize);
            if (result.Success && result.Data != null)
            {
                _pagedResult = result.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load FAQs.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading FAQs.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task NextPage()
    {
        if (_pagedResult.HasNextPage)
        {
            _currentPage++;
            await LoadFaqsAsync();
        }
    }

    private async Task PreviousPage()
    {
        if (_pagedResult.HasPreviousPage)
        {
            _currentPage--;
            await LoadFaqsAsync();
        }
    }

    private void OpenAddForm()
    {
        _currentFaq = new();
        _isEditing = false;
        _showForm = true;
    }

    private void EditFaq(FaqModel faq)
    {
        _currentFaq = new FaqModel
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer
        };
        _isEditing = true;
        _showForm = true;
    }

    private void CancelEdit()
    {
        _showForm = false;
        _currentFaq = new();
    }

    private async Task SaveFaq()
    {
        if (string.IsNullOrWhiteSpace(_currentFaq.Question) || string.IsNullOrWhiteSpace(_currentFaq.Answer)) return;

        _saving = true;

        try
        {
            ApiResult<bool> result;
            if (_isEditing)
                result = await FAQService.UpdateAsync(_currentFaq);
            else
                result = await FAQService.CreateAsync(_currentFaq);

            if (result.Success)
            {
                ToastService.ShowSuccess($"FAQ {(_isEditing ? "updated" : "created")} successfully.");
                await LoadFaqsAsync();
                _showForm = false;
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to save FAQ.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while saving the FAQ.");
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteFaq(Guid id)
    {
        try
        {
            var result = await FAQService.DeleteAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("FAQ deleted successfully.");
                await LoadFaqsAsync();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to delete FAQ.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while deleting the FAQ.");
        }
    }
}
