using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.Models;

namespace TickAPI.Organizers.Abstractions;

public interface IOrganizerRepository
{
    Task<Result<Organizer>> GetOrganizerByEmailAsync(string organizerEmail);
    Task AddNewOrganizerAsync(Organizer organizer);
    Task<Result> VerifyOrganizerByEmailAsync(string organizerEmail);
    IQueryable<Organizer> GetOrganizers();
}