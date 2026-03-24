using Microsoft.AspNetCore.Components;
using Bookify.Client.Models;
using Bookify.Client.Models.SupportTicket;
using Bookify.Client.Services;

namespace Bookify.Client.Pages.Admin;

public partial class ManageTickets : ComponentBase
{
    [Inject] private ITicketApiService TicketService { get; set; } = default!;
    [Inject] private ToastService ToastService { get; set; } = default!;

    private PagedResult<TicketModel> _pagedResult = new();
    private bool _loading = true;
    private Guid? _expandedTicketId;
    private int _currentPage = 1;
    private const int PageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadTickets();
    }

    private async Task LoadTickets()
    {
        _loading = true;
        try
        {
            var result = await TicketService.GetAllAsync(_currentPage, PageSize);
            if (result.Success && result.Data != null)
            {
                _pagedResult = result.Data;
            }
            else
            {
                ToastService.ShowError("Failed to load tickets.");
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("An unexpected error occurred while loading tickets.");
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
            await LoadTickets();
            _expandedTicketId = null;
        }
    }

    private async Task PreviousPage()
    {
        if (_pagedResult.HasPreviousPage)
        {
            _currentPage--;
            await LoadTickets();
            _expandedTicketId = null;
        }
    }

    private async Task ToggleTicket(TicketModel ticket)
    {
        if (_expandedTicketId == ticket.Id)
        {
            _expandedTicketId = null;
        }
        else
        {
            _expandedTicketId = ticket.Id;
            if (!ticket.IsRead)
            {
                try
                {
                    var result = await TicketService.MarkAsReadAsync(ticket.Id);
                    if (result.Success)
                    {
                        ticket.IsRead = true;
                    }
                }
                catch (Exception)
                {
                    ToastService.ShowError("Failed to mark ticket as read.");
                }
            }
        }
    }
}
