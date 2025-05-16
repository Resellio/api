using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.TicketTypes.Models;

namespace TickAPI.TicketTypes.Abstractions;

public interface ITicketTypeRepository
{
    public Result<TicketType> GetTicketTypeById(Guid ticketTypeId);
}