using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Category;
using Bookify.Client.Models.Service;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Services;

public partial class ServiceList : ComponentBase
{
    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<ServiceModel>  _services          = [];
    private List<CategoryModel> _categories        = [];
    private bool                _loading           = true;
    private string              _search            = string.Empty;
    private Guid?               _selectedCategoryId;
    private int                 _currentPage       = 1;
    private int                 _totalPages        = 1;
    private int                 _totalCount        = 0;
    private const int           PageSize           = 12;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var catResult = await CategoryService.GetAllAsync();
            if (catResult.Success)
            {
                _categories = catResult.Data ?? [];
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("Could not load categories.");
        }

        await LoadServicesAsync();
    }

    private async Task LoadServicesAsync()
    {
        _loading = true;
        try
        {
            var svcResult = await ServiceService.GetAllAsync(_search, _currentPage, PageSize);
            if (svcResult.Success)
            {
                var items = svcResult.Data?.Items ?? [];
                
                if (_selectedCategoryId.HasValue)
                {
                    items = items.Where(s => s.CategoryId == _selectedCategoryId.Value).ToList();
                }

                _services  = items.ToList();
                _totalPages = svcResult.Data?.TotalPages ?? 1;
                _totalCount = svcResult.Data?.TotalCount ?? 0;
            }
            else
            {
                ToastService.ShowError(svcResult.Message ?? "Failed to load services.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading services.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnPageChanged(int page)
    {
        _currentPage = page;
        await LoadServicesAsync();
    }

    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? "";
        _currentPage = 1;
        await LoadServicesAsync();
    }

    private async Task SelectCategory(Guid? id)
    {
        _selectedCategoryId = id;
        _currentPage = 1;
        await LoadServicesAsync();
    }
}
