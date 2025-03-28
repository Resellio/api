using TickAPI.Common.Results.Generic;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;

namespace TickAPI.Events.Abstractions;

public interface IAddressRepository
{
    public Task<Result<Address>> GetAddressAsync(CreateAddressDto createAddress);
}