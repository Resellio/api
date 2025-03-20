using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;

namespace TickAPI.Organizers.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public OrganizerRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    
    public async Task<Result<Organizer>> GetOrganizerByEmailAsync(string organizerEmail)
    {
        var organizer = await _tickApiDbContext.Organizers.FirstOrDefaultAsync(organizer => organizer.Email == organizerEmail);

        if (organizer == null)
        {
            return Result<Organizer>.Failure(StatusCodes.Status404NotFound, $"organizer with email '{organizerEmail}' not found");
        }
        
        return Result<Organizer>.Success(organizer);
    }

    public async Task AddNewOrganizerAsync(Organizer organizer)
    {
        _tickApiDbContext.Organizers.Add(organizer);
        await _tickApiDbContext.SaveChangesAsync();
    }

    public async Task<Result> VerifyOrganizerByEmailAsync(string organizerEmail)
    {
        var organizer = await _tickApiDbContext.Organizers.FirstOrDefaultAsync(organizer => organizer.Email == organizerEmail);

        if (organizer == null)
        {
            return Result.Failure(StatusCodes.Status404NotFound, $"organizer with email '{organizerEmail}' not found");
        }

        if (organizer.IsVerified)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"organizer with email '{organizerEmail}' is already verified");
        }
        
        organizer.IsVerified = true;
        await _tickApiDbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}