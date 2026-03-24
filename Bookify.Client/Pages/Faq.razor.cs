using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.FAQ;
using Bookify.Client.Models;
using Bookify.Client.Services;

namespace Bookify.Client.Pages;

public partial class Faq : ComponentBase
{
    [Inject] private IFAQApiService FAQService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private PagedResult<FaqModel> _pagedResult = new();
    private bool _loadingFaqs = true;
    private HashSet<Guid> _expandedFaqs = new();
    private int _currentPage = 1;
    private const int PageSize = 5;
    private string _searchQuery = string.Empty;

    private IEnumerable<FaqModel> FilteredFaqs => _pagedResult.Items;

    protected override async Task OnInitializedAsync()
    {
        await LoadFaqsAsync();
    }

    private async Task LoadFaqsAsync()
    {
        _loadingFaqs = true;
        
        try
        {
            var result = await FAQService.GetAllAsync(_currentPage, PageSize, _searchQuery);
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
            _loadingFaqs = false;
        }
    }

    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
        _expandedFaqs.Clear();
        await LoadFaqsAsync();
    }

    private async Task NextPage()
    {
        if (_pagedResult.HasNextPage)
        {
            _currentPage++;
            await LoadFaqsAsync();
            _expandedFaqs.Clear();
        }
    }

    private async Task PreviousPage()
    {
        if (_pagedResult.HasPreviousPage)
        {
            _currentPage--;
            await LoadFaqsAsync();
            _expandedFaqs.Clear();
        }
    }

    private void ToggleFaq(Guid id)
    {
        if (_expandedFaqs.Contains(id)) _expandedFaqs.Remove(id);
        else _expandedFaqs.Add(id);
    }

    private bool IsExpanded(Guid id) => _expandedFaqs.Contains(id);
}
