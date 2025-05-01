using TickAPI.Admins.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Admins.Abstractions;

public interface IAdminRepository
{
    Task<Result<Admin>> GetAdminByEmailAsync(string adminEmail);
}