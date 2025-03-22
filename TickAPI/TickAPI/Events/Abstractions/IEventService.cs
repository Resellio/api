using TickAPI.Events.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Events.Abstractions;

public interface IEventService
{
    public Task<Result<Event>> CreateNewEventAsync(string name, string description, DateTime startDate,
        DateTime endDate, uint? minimumAge, string organizerEmail, Address address);
}