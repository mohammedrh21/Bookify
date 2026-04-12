using Microsoft.AspNetCore.Components;
using Bookify.Client.Models;
using Bookify.Client.Models.FAQ;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin.Manage;

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
    private HashSet<Guid> _expandedFaqIds = new();
    private bool _showDeleteModal = false;
    private Guid? _faqIdToDelete = null;
    private int _currentPage = 1;
    private const int PageSize = 10;

    private void ToggleFaq(Guid id)
    {
        if (_expandedFaqIds.Contains(id))
            _expandedFaqIds.Remove(id);
        else
            _expandedFaqIds.Add(id);
    }

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
                // errors are already shown by BaseApiService
            }
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            _saving = false;
        }
    }

    private void DeleteFaq(Guid id)
    {
        _faqIdToDelete = id;
        _showDeleteModal = true;
    }

    private async Task HandleDeleteConfirmed()
    {
        if (!_faqIdToDelete.HasValue) return;

        try
        {
            var result = await FAQService.DeleteAsync(_faqIdToDelete.Value);
            if (result.Success)
            {
                ToastService.ShowSuccess("FAQ deleted successfully.");
                await LoadFaqsAsync();
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
            _showDeleteModal = false;
            _faqIdToDelete = null;
        }
    }

    private void HandleDeleteCanceled()
    {
        _showDeleteModal = false;
        _faqIdToDelete = null;
    }
}
