using Microsoft.EntityFrameworkCore;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Addresses.Models;


namespace TickAPI.Addresses.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public AddressRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }


    public async Task<Result<Address>> GetAddressAsync(CreateAddressDto createAddress)
    {
        var address = await _tickApiDbContext.Addresses.FirstOrDefaultAsync(x => 
            x.Street == createAddress.Street &&
            x.City == createAddress.City &&
            x.Country == createAddress.Country &&
            x.HouseNumber == createAddress.HouseNumber &&
            x.PostalCode == createAddress.PostalCode &&
            x.FlatNumber == createAddress.FlatNumber
            );
        
        if (address == null)
        {
            return Result<Address>.Failure(StatusCodes.Status404NotFound,"Address not found");
        }
        
        return Result<Address>.Success(address);    
    }
}