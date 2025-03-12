namespace TickAPI.Common.Auth.Abstractions;

public interface IJwtService
{
    public string GenerateJwtToken(string userEmail, string role);
}