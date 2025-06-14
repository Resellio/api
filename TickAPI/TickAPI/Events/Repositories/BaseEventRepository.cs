﻿using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Events.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Models;

namespace TickAPI.Events.Repositories;

public class BaseEventRepository : IEventRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public BaseEventRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    
    public async Task AddNewEventAsync(Event @event)
    {
        _tickApiDbContext.Events.Add(@event);
        await _tickApiDbContext.SaveChangesAsync();
    }

    public async Task<IQueryable<Event>> GetEventsAsync()
    {
        return _tickApiDbContext.Events
            .Include(e => e.Address)
            .Include(e => e.TicketTypes)
            .Include(e => e.Categories);
    }

    public async Task<IQueryable<Event>> GetEventsByOranizerAsync(Organizer organizer)
    {
        return _tickApiDbContext.Events
            .Include(e => e.Address)
            .Include(e => e.TicketTypes)
            .Include(e => e.Categories)
            .Where(e => e.Organizer.Id == organizer.Id);
    }

    public async Task<Result<Event>> GetEventByIdAsync(Guid eventId)
    {
        var @event = await _tickApiDbContext.Events
            .Include(e => e.Address)
            .Include(e => e.TicketTypes)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (@event == null)
        {
            return Result<Event>.Failure(StatusCodes.Status404NotFound, $"event with id {eventId} not found");
        }
        
        return Result<Event>.Success(@event);
    }

    public async Task<Result> SaveEventAsync(Event ev)
    {
        var fromDb = await GetEventByIdAsync(ev.Id);
        if (fromDb.IsError)
            return Result.PropagateError(fromDb);
        await _tickApiDbContext.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<Event>> GetEventByIdAndOrganizerAsync(Guid eventId, Organizer organizer)
    {
        var organizerEvents = GetEventsByOranizerAsync(organizer);
        var ev = await (await organizerEvents).Where(e => e.Id == eventId).FirstAsync();
        if (ev is null)
        {
            return Result<Event>.Failure(StatusCodes.Status404NotFound, $"Event with id {eventId} not found for organizer with id {organizer.Id}");
        }
        return Result<Event>.Success(ev);
    }

    public async Task<decimal> GetEventRevenue(Guid eventId)
    {
        var query = from tickets in _tickApiDbContext.Tickets
            join _ticketTypes in _tickApiDbContext.TicketTypes on tickets.Type.Id equals _ticketTypes.Id
            join events in _tickApiDbContext.Events on _ticketTypes.Event.Id equals events.Id
            where events.Id == eventId
            select new { price = _ticketTypes.Price };
        var val = await query.SumAsync(x => x.price);
        return val;
    }
    
    public async Task<int> GetEventSoldTicketsCount(Guid eventId)
    {
        var query = from tickets in _tickApiDbContext.Tickets
            join _ticketTypes in _tickApiDbContext.TicketTypes on tickets.Type.Id equals _ticketTypes.Id
            join events in _tickApiDbContext.Events on _ticketTypes.Event.Id equals events.Id
            where events.Id == eventId
            select new { id = tickets.Id };
        var val = await query.CountAsync();
        return val;
    }
}