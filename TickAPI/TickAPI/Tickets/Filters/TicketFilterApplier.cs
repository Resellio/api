using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.DTOs.Request;
using TickAPI.Tickets.Models;

namespace TickAPI.Tickets.Filters;

public class TicketFilterApplier
{
    private readonly ITicketFilter _ticketFilter;
    private readonly Dictionary<Func<TicketFiltersDto, bool>, Action<TicketFiltersDto>> _filterActions;

    public TicketFilterApplier(ITicketFilter ticketFilter)
    {
        _ticketFilter = ticketFilter;
        _filterActions = new Dictionary<Func<TicketFiltersDto, bool>, Action<TicketFiltersDto>>
        {
            { f => !string.IsNullOrEmpty(f.EventName), f => _ticketFilter.FilterTicketsByEventName(f.EventName!) },
            { f => f.usedOnly, f => _ticketFilter.FilterUsedTickets() },
            { f => f.unusedOnly, f => _ticketFilter.FilterUnusedTickets() },
            { f => f.forResellOnly, f => _ticketFilter.FilterTicketsForResell() },
            { f => f.notForResellOnly, f => _ticketFilter.FilterTicketsNotForResell() },
        };
    }
    
    public IQueryable<Ticket> ApplyFilters(TicketFiltersDto filters)
    {   
        foreach (var (condition, apply) in _filterActions)
        {
            if (condition(filters))
            {
                apply(filters);
            }
        }
        
        return _ticketFilter.GetTickets();
    }
}