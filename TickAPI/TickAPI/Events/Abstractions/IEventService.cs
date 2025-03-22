using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Request;

namespace TickAPI.Events.Abstractions;

public interface IEventService
{
    public Task<Result<Event>> CreateNewEventAsync(string name, string description, string startDate,
        string endDate, uint? minimumAge, AddressDto address, string organizerEmail);
}