using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Service;
using Bookify.Client.Models.Category;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Staff;

public partial class UpdateService : ComponentBase
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IServiceApiService ServiceService { get; set; } = default!;
    [Inject] private ICategoryService CategoryService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private ServiceModel? _model = new();
    private ServiceModel _modelUpdate = new();

    private List<CategoryModel> _categories = new();
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        var svcResult  = await ServiceService.GetByIdAsync(Id);
        _model         = svcResult.Data;
        var catResult  = await CategoryService.GetAllAsync();
        _categories    = catResult.Data ?? [];
        _loading = false;
    }

    private async Task HandleSubmit()
    {
        if (_model != null)
            _modelUpdate = _model;

        var result = await ServiceService.UpdateAsync(_modelUpdate);
        if (result.Success)
            Nav.NavigateTo("/services/my-service");
    }
}
