using Microsoft.AspNetCore.Mvc;
using TickAPI.Common.Auth.Attributes;
using TickAPI.Common.Auth.Enums;

namespace TickAPI.Tickets.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    public TicketsController()
    {
        
    }
    
    // [AuthorizeWithPolicy(AuthPolicies.CustomerPolicy)]
    // [HttpGet("{id:guid}")]
    
    
}