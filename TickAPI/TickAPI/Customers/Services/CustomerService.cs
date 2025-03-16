using TickAPI.Common.Result;
using TickAPI.Common.Time.Abstractions;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;
using TickAPI.Tickets.Models;

namespace TickAPI.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDateTimeService _dateTimeService;

    public CustomerService(ICustomerRepository customerRepository, IDateTimeService dateTimeService)
    {
        _customerRepository = customerRepository;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<Customer>> GetCustomerByEmailAsync(string customerEmail)
    {
        return await _customerRepository.GetCustomerByEmailAsync(customerEmail);
    }

    public async Task<Result<Customer>> CreateNewCustomerAsync(string email, string firstName, string lastName)
    {
        var alreadyExistingResult = await GetCustomerByEmailAsync(email);
        if (alreadyExistingResult.IsSuccess)
            return Result<Customer>.Failure(StatusCodes.Status400BadRequest,
                $"customer with email '{email}' already exists");
        var customer = new Customer
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreationDate = _dateTimeService.GetCurrentDateTime(),
            Tickets = new List<Ticket>()
        };
        await _customerRepository.AddNewCustomerAsync(customer);
        return Result<Customer>.Success(customer);
    }
}