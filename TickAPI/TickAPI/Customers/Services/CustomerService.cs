using TickAPI.Common.Result;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;

namespace TickAPI.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<Customer>> GetCustomerByEmailAsync(string customerEmail)
    {
        return await _customerRepository.GetCustomerByEmailAsync(customerEmail);
    }
}