using TickAPI.Common.Pagination.Abstractions;
using TickAPI.Common.Pagination.Responses;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.DTOs.Response;
using TickAPI.Organizers.Models;

namespace TickAPI.Organizers.Services;

public class OrganizerService : IOrganizerService
{
    private readonly IOrganizerRepository _organizerRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly IPaginationService _paginationService;

    public OrganizerService(IOrganizerRepository organizerRepository, IDateTimeService dateTimeService, IPaginationService paginationService)
    {
        _organizerRepository = organizerRepository;
        _dateTimeService = dateTimeService;
        _paginationService = paginationService;
    }
    
    public async Task<Result<Organizer>> GetOrganizerByEmailAsync(string organizerEmail)
    {
        return await _organizerRepository.GetOrganizerByEmailAsync(organizerEmail);
    }

    public async Task<Result<Organizer>> CreateNewOrganizerAsync(string email, string firstName, string lastName, string displayName)
    {
        var alreadyExistingResult = await GetOrganizerByEmailAsync(email);
        if (alreadyExistingResult.IsSuccess)
            return Result<Organizer>.Failure(StatusCodes.Status400BadRequest,
                $"organizer with email '{email}' already exists");

        var organizer = new Organizer
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            IsVerified = false,
            CreationDate = _dateTimeService.GetCurrentDateTime(),
            Events = new List<Event>()
        };
        await _organizerRepository.AddNewOrganizerAsync(organizer);
        return Result<Organizer>.Success(organizer);
    }

    public async Task<Result> VerifyOrganizerByEmailAsync(string organizerEmail)
    {
        return await _organizerRepository.VerifyOrganizerByEmailAsync(organizerEmail);
    }

    public async Task<Result<PaginatedData<GetUnverifiedOrganizerResponseDto>>> GetUnverifiedOrganizersAsync(int page, int pageSize)
    {
        var unverifiedOrganizers = _organizerRepository.GetOrganizers().Where(o => !o.IsVerified);
        var paginatedResult = await _paginationService.PaginateAsync(unverifiedOrganizers, pageSize, page);
        if (paginatedResult.IsError)
        {
            return Result<PaginatedData<GetUnverifiedOrganizerResponseDto>>.PropagateError(paginatedResult);
        }
        var paginated = paginatedResult.Value!;
        var mapped = _paginationService.MapData(paginated, (o) => new GetUnverifiedOrganizerResponseDto(o.Email, o.FirstName, o.LastName, o.DisplayName));
        return Result<PaginatedData<GetUnverifiedOrganizerResponseDto>>.Success(mapped);
    }
}