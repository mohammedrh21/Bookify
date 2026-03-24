using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Category;
using Bookify.Client.Models.Service;
using Bookify.Client.Services;

namespace Bookify.Client.Pages;

public partial class Home : ComponentBase
{
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<CategoryModel> _categories = [];
    private List<ServiceModel>  _allServices = [];
    private bool _loadingCategories = true;
    private bool _loadingServices = true;
    private Guid? _selectedCategoryId;

    private List<ServiceModel> _services => _selectedCategoryId.HasValue
        ? _allServices.Where(s => s.CategoryId == _selectedCategoryId.Value && !s.IsDeleted).ToList()
        : _allServices.Where(s => !s.IsDeleted).ToList();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var catTask = CategoryService.GetAllAsync();
            var svcTask = ServiceService.GetAllAsync();

            await Task.WhenAll(catTask, svcTask);

            var catResult = catTask.Result;
            var svcResult = svcTask.Result;

            _categories = catResult.Data ?? [];
            _allServices = svcResult.Data?.Items.ToList() ?? [];
        }
        catch (Exception)
        {
            ToastService.ShowError("Failed to load some data. Please refresh the page.");
        }
        finally
        {
            _loadingCategories = false;
            _loadingServices = false;
        }
    }

    private void SelectCategory(Guid id)
    {
        _selectedCategoryId = _selectedCategoryId == id ? null : id;
    }
}
