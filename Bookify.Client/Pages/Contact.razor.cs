using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.ContactInfo;
using Bookify.Client.Services;

namespace Bookify.Client.Pages;

public partial class Contact : ComponentBase
{
    [Inject] private IContactInfoApiService ContactInfoService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private ContactInfoModel? _info;
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var result = await ContactInfoService.GetAsync();
            if (result.Success)
            {
                _info = result.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load contact info.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading contact info.");
        }
        finally
        {
            _loading = false;
        }
    }
}
