using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Events.Models;
using TickAPI.Organizers.Abstractions;
using TickAPI.Organizers.Models;

namespace TickAPI.Organizers.Services;

public class OrganizerService : IOrganizerService
{
    private readonly IOrganizerRepository _organizerRepository;
    private readonly IDateTimeService _dateTimeService;

    public OrganizerService(IOrganizerRepository organizerRepository, IDateTimeService dateTimeService)
    {
        _organizerRepository = organizerRepository;
        _dateTimeService = dateTimeService;
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
}