using Microsoft.EntityFrameworkCore;
using TickAPI.Admins.Abstractions;
using TickAPI.Admins.Models;
using TickAPI.Common.Results.Generic;
using TickAPI.Common.TickApiDbContext;
using TickAPI.Customers.Models;

namespace TickAPI.Admins.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly TickApiDbContext _tickApiDbContext;

    public AdminRepository(TickApiDbContext tickApiDbContext)
    {
        _tickApiDbContext = tickApiDbContext;
    }
    public async Task<Result<Admin>> GetAdminByEmailAsync(string adminEmail)
    {
        var admin = await _tickApiDbContext.Admins.FirstOrDefaultAsync(admin => admin.Email == adminEmail);
        
        if (admin == null)
        {
            return Result<Admin>.Failure(StatusCodes.Status404NotFound, $"admin with email '{adminEmail}' not found");
        }

        return Result<Admin>.Success(admin);
    }
}