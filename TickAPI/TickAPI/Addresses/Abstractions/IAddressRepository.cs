using TickAPI.Addresses.DTOs.Request;
using TickAPI.Common.Results.Generic;
using TickAPI.Addresses.Models;

namespace TickAPI.Addresses.Abstractions;

public interface IAddressRepository
{
    public Task<Result<Address>> GetAddressAsync(CreateAddressDto createAddress);
}