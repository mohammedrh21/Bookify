using Microsoft.AspNetCore.Components;
using Bookify.Client.Models.Common;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin.Clients;

public partial class ManageClients : ComponentBase
{
    [Inject] private IUserApiService UserApiService { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    private List<ClientSummaryModel> _clients = [];
    private ClientReportModel? _report;
    private Guid? _selectedClientId;
    private bool _loading = true;
    private bool _reportLoading = false;
    private int _clientPage = 1;
    private int _totalClientPages = 1;
    private int _totalClientCount = 0;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadClients();
    }

    private async Task LoadClients()
    {
        _loading = true;
        try
        {
            var result = await UserApiService.GetClientsAsync(_clientPage, PageSize);
            _clients = result.Items.ToList();
            _totalClientPages = result.TotalPages;
            _totalClientCount = result.TotalCount;
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

    private async Task OnClientPageChanged(int page)
    {
        _clientPage = page;
        await LoadClients();
    }

    private async Task LoadReport(Guid clientId)
    {
        _selectedClientId = clientId;
        _reportLoading = true;
        try
        {
            _report = await UserApiService.GetClientReportAsync(clientId);
        }
        catch (Exception)
        {
            // unexpected error
        }
        finally
        {
            _reportLoading = false;
        }
    }

    private async Task ToggleActive()
    {
        if (_selectedClientId == null) return;
        try
        {
            var success = await UserApiService.ToggleUserActiveAsync(_selectedClientId.Value);
            if (success)
            {
                if (_report != null) _report.IsActive = !_report.IsActive;
                var client = _clients.FirstOrDefault(c => c.Id == _selectedClientId);
                if (client != null) client.IsActive = !client.IsActive;
                Toast.ShowSuccess("Client status updated.");
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
