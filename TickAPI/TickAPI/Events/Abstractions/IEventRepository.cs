using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IEventRepository
{
    public Task AddNewEventAsync(Event @event);
}