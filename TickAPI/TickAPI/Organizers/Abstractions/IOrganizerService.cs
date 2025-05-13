using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Organizers.DTOs.Response;
using TickAPI.Organizers.Models;

namespace TickAPI.Organizers.Abstractions;

public interface IOrganizerService
{
    public Task<Result<Organizer>> GetOrganizerByEmailAsync(string organizerEmail);
    
    public Task<Result<Organizer>> CreateNewOrganizerAsync(string email, string firstName, string lastName, string displayName);

    public Task<Result> VerifyOrganizerByEmailAsync(string organizerEmail);

    public Task<Result<PaginatedData<GetUnverifiedOrganizerResponseDto>>> GetUnverifiedOrganizersAsync(int page, int pageSize);
}