using TickAPI.Admins.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Admins.Abstractions;

public interface IAdminService
{
    public Task<Result<Admin>> GetAdminByEmailAsync(string adminEmail);
}