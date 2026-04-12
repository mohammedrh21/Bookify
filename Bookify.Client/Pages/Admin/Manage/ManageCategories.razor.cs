using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Category;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin.Manage;

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
                // errors are already shown by BaseApiService
            }
        }
        catch (Exception)
        {
            // unexpected error
        }
    }
}
