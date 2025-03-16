using TickAPI.Common.Result;
using TickAPI.Customers.Models;

namespace TickAPI.Customers.Abstractions;

public interface ICustomerService
{
    Task<Result<Customer>> GetCustomerByEmailAsync(string customerEmail);
}