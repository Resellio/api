using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Abstractions;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;
using TickAPI.Addresses.Abstractions;
using TickAPI.Addresses.DTOs.Request;
using TickAPI.Addresses.Models;

namespace TickAPI.Addresses.Services;

public class AddressService : IAddressService
{
    
    private readonly IAddressRepository _addressRepository;

    public AddressService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }
    public async Task<Result<Address>> GetOrCreateAddressAsync(CreateAddressDto createAddress)
    {
        var  result = await _addressRepository.GetAddressAsync(createAddress);
        if (result.IsSuccess)
        {
            return Result<Address>.Success(result.Value!);
        }

        return Result<Address>.Success(FromDto(createAddress));
    }
    
    private static Address FromDto(CreateAddressDto dto)
    {
        return new Address
        {
            City = dto.City,
            HouseNumber = dto.HouseNumber,
            FlatNumber = dto.FlatNumber,
            PostalCode = dto.PostalCode,
            Street = dto.Street,
            Country = dto.Country
        };
    }
    
}