﻿using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;

namespace TickAPI.Customers.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public CustomerRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }

    public async Task<Result<Customer>> GetCustomerByEmailAsync(string customerEmail)
    {
        var customer = await _tickApiDbContext.Customers.FirstOrDefaultAsync(customer => customer.Email == customerEmail);
        
        if (customer == null)
        {
            return Result<Customer>.Failure(StatusCodes.Status404NotFound, $"customer with email '{customerEmail}' not found");
        }

        return Result<Customer>.Success(customer);
    }

    public async Task AddNewCustomerAsync(Customer customer)
    {
        _tickApiDbContext.Customers.Add(customer);
        await _tickApiDbContext.SaveChangesAsync();
    }

    public IQueryable<Customer> GetCustomersWithTicketForEvent(Guid eventId)
    {
        return _tickApiDbContext.Customers
            .Where(c => c.Tickets.Any(t => t.Type.Event.Id == eventId));
    }
}