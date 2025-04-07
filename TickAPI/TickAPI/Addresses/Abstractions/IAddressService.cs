using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Results.Generic;
using TickAPI.Addresses.Models;

namespace TickAPI.Addresses.Abstractions;

public interface IAddressService
{
    public Task<Result<Address>> GetOrCreateAddressAsync(CreateAddressDto createAddress);
}