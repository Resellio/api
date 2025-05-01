using TickAPI.Admins.Abstractions;
using TickAPI.Admins.Models;
using TickAPI.Common.Results.Generic;

namespace TickAPI.Admins.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<Result<Admin>> GetAdminByEmailAsync(string adminEmail)
    {
        return await _adminRepository.GetAdminByEmailAsync(adminEmail);
    }
}