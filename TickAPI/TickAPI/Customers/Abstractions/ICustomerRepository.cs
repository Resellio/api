using TickAPI.Common.Result;
using TickAPI.Customers.Models;

namespace TickAPI.Customers.Abstractions;

public interface ICustomerRepository
{
    Task<Result<Customer>> GetCustomerByEmailAsync(string customerEmail);
    Task AddNewCustomerAsync(Customer customer);
}