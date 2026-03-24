using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Category;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ManageCategories : ComponentBase
{
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private List<CategoryModel> _categories      = [];
    private string              _newCategoryName  = string.Empty;
    private bool                _loading          = true;
    private bool                _showForm         = false;
    private bool                _saving           = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await CategoryService.GetAllAsync();
            if (result.Success)
            {
                _categories = result.Data ?? [];
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to load categories.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading categories.");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task CreateCategory()
    {
        if (string.IsNullOrWhiteSpace(_newCategoryName)) return;

        _saving = true;
        StateHasChanged();

        try
        {
            var result = await CategoryService.CreateAsync(new CategoryModel { Name = _newCategoryName.Trim() });
            if (result.Success)
            {
                ToastService.ShowSuccess("Category created.");
                var refresh = await CategoryService.GetAllAsync();
                _categories      = refresh.Data ?? [];
                _newCategoryName = string.Empty;
                _showForm        = false;
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to create category.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while creating the category.");
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeactivateCategory(Guid id)
    {
        try
        {
            var result = await CategoryService.DeactivateAsync(id);
            if (result.Success)
            {
                ToastService.ShowSuccess("Category deactivated.");
                var refresh = await CategoryService.GetAllAsync();
                if (refresh.Success)
                    _categories = refresh.Data ?? [];
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Failed to deactivate category.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while deactivating the category.");
        }
    }
}
