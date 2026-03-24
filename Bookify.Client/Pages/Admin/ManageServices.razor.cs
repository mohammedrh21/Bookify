using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Category;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ManageServices : ComponentBase
{
    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<ServiceModel> _services = [];
    private List<CategoryModel> _categories = [];
    private bool _loading = true;
    private string _search = string.Empty;
    private string _categoryFilter = string.Empty;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount = 0;
    private const int PageSize =10;

    protected override async Task OnInitializedAsync()
    {
        var catResult = await CategoryService.GetAllAsync();
        _categories = catResult.Data ?? [];
        await LoadServicesAsync();
    }

    private async Task LoadServicesAsync()
    {
        _loading = true;
        var searchTerm = _search;

        try
        {
            var result = await ServiceService.GetAllAsync(searchTerm, _currentPage, PageSize);
            
            if (result.Success)
            {
                var items = result.Data?.Items ?? [];

                if (Guid.TryParse(_categoryFilter, out var catId))
                    items = items.Where(s => s.CategoryId == catId).ToList();

                _services = items.ToList();
                _totalPages = result.Data?.TotalPages ?? 1;
                _totalCount = result.Data?.TotalCount ?? 0;
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to load services.");
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

    private async Task OnCategoryChanged()
    {
        _currentPage = 1;
        await LoadServicesAsync();
    }

    private async Task DeleteService(Guid id)
    {
        try
        {
            var result = await ServiceService.DeleteAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Service deleted successfully.");
                await LoadServicesAsync();
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
    }
}
