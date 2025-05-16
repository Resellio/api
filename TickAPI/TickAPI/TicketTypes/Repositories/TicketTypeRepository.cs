using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.TicketTypes.Abstractions;
using TickAPI.TicketTypes.Models;

namespace TickAPI.TicketTypes.Repositories;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public TicketTypeRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    
    public Result<TicketType> GetTicketTypeById(Guid ticketTypeId)
    {
        var ticketType =
            _tickApiDbContext.TicketTypes
                .FirstOrDefault(t => t.Id == ticketTypeId);

        if (ticketType == null)
        {
            return Result<TicketType>.Failure(StatusCodes.Status404NotFound,$"ticket type with id {ticketTypeId} not found");
        }
        
        return Result<TicketType>.Success(ticketType);
    }
}