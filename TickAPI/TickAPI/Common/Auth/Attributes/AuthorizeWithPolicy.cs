using Microsoft.AspNetCore.Authorization;
using TickAPI.Common.Auth.Enums;

namespace TickAPI.Common.Auth.Attributes;

public class AuthorizeWithPolicy : AuthorizeAttribute
{
    public AuthorizeWithPolicy(AuthPolicies policy)
    {
        Policy = policy.ToString();
    }
}