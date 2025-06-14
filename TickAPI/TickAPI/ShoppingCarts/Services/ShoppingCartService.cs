using Google.Apis.Auth.OAuth2.Web;
using TickAPI.Common.Payment.Abstractions;
using TickAPI.Common.Payment.Models;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Customers.Abstractions;
using TickAPI.Customers.Models;
using TickAPI.Events.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.ShoppingCarts.Mappers;
using TickAPI.ShoppingCarts.Models;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Models;
using TickAPI.TicketTypes.Abstractions;

namespace TickAPI.ShoppingCarts.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITicketService _ticketService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, ICustomerRepository customerRepository, 
        ITicketService ticketService, IPaymentGatewayService paymentGatewayService)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _customerRepository = customerRepository;
        _ticketService = ticketService;
        _paymentGatewayService = paymentGatewayService;
    }
    
    public async Task<Result> AddNewTicketsToCartAsync(Guid ticketTypeId, uint amount, string customerEmail)
    {
        var availabilityResult = await _ticketService.CheckTicketAvailabilityByTypeIdAsync(ticketTypeId, amount);

        if (availabilityResult.IsError)
        {
            return Result.PropagateError(availabilityResult);
        }

        if (!availabilityResult.Value)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"not enough available tickets of type {ticketTypeId}");
        }
        
        var addTicketsToCartResult = await _shoppingCartRepository.AddNewTicketsToCartAsync(customerEmail, ticketTypeId, amount);

        if (addTicketsToCartResult.IsError)
        {
            return Result.PropagateError(addTicketsToCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result> AddResellTicketToCartAsync(Guid ticketId, string customerEmail)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(ticketId);

        if (ticketResult.IsError)
        {
            return Result.PropagateError(ticketResult);
        }
        
        var ticket = ticketResult.Value!;

        if (!ticket.ForResell)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"chosen ticket is not available for resell");
        }

        if (ticket.Owner.Email == customerEmail)
        {
            return Result.Failure(StatusCodes.Status403Forbidden, "you can't buy ticket sold from your account");
        }
        
        var availabilityResult = await _shoppingCartRepository.CheckResellTicketAvailabilityAsync(ticketId);

        if (availabilityResult.IsError)
        {
            return Result.PropagateError(availabilityResult);
        }

        if (!availabilityResult.Value)
        {
            return Result.Failure(StatusCodes.Status400BadRequest, $"the ticket you are trying to add isn't currently available");
        }
        
        var addTicketToCartResult = await _shoppingCartRepository.AddResellTicketToCartAsync(customerEmail, ticketId);
        
        if (addTicketToCartResult.IsError)
        {
            return Result.PropagateError(addTicketToCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<GetShoppingCartTicketsResponseDto>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var newTickets = new List<GetShoppingCartTicketsNewTicketDetailsResponseDto>();
        var resellTickets = new List<GetShoppingCartTicketsResellTicketDetailsResponseDto>();

        foreach (var ticket in cart.NewTickets)
        {
            var newTicketResult = await _ticketService.GetTicketTypeByIdAsync(ticket.TicketTypeId);

            if (newTicketResult.IsError)
            {
                return Result<GetShoppingCartTicketsResponseDto>.PropagateError(newTicketResult);
            }

            var newTicket =
                ShoppingCartMapper.MapTicketTypeToGetShoppingCartTicketsNewTicketDetailsResponseDto(
                    newTicketResult.Value!, ticket.Quantity);
            
            newTickets.Add(newTicket);
        }

        foreach (var ticket in cart.ResellTickets)
        {
            var resellTicketResult = await _ticketService.GetTicketByIdAsync(ticket.TicketId);

            if (resellTicketResult.IsError)
            {
                return Result<GetShoppingCartTicketsResponseDto>.PropagateError(resellTicketResult);
            }

            var resellTicket =
                ShoppingCartMapper.MapTicketToGetShoppingCartTicketsResellTicketDetailsResponseDto(resellTicketResult
                    .Value!);
            
            resellTickets.Add(resellTicket);
        }
        
        var result = new GetShoppingCartTicketsResponseDto(newTickets, resellTickets);
        
        return Result<GetShoppingCartTicketsResponseDto>.Success(result);
    }

    public async Task<Result> RemoveNewTicketsFromCartAsync(Guid ticketTypeId, uint amount, string customerEmail)
    {
        var removeTicketsFromCartResult = await _shoppingCartRepository.RemoveNewTicketsFromCartAsync(customerEmail, ticketTypeId, amount);

        if (removeTicketsFromCartResult.IsError)
        {
            return Result.PropagateError(removeTicketsFromCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result> RemoveResellTicketFromCartAsync(Guid ticketId, string customerEmail)
    {
        var removeTicketFromCartResult = await _shoppingCartRepository.RemoveResellTicketFromCartAsync(customerEmail, ticketId);

        if (removeTicketFromCartResult.IsError)
        {
            return Result.PropagateError(removeTicketFromCartResult);
        }
        
        return Result.Success();
    }

    public async Task<Result<Dictionary<string, decimal>>> GetDueAmountAsync(string customerEmail)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<Dictionary<string, decimal>>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;

        Dictionary<string, decimal> dueAmount = new Dictionary<string, decimal>();

        foreach (var newTicket in cart.NewTickets)
        {
            var ticketTypeResult = await _ticketService.GetTicketTypeByIdAsync(newTicket.TicketTypeId);

            if (ticketTypeResult.IsError)
            {
                return Result<Dictionary<string, decimal>>.PropagateError(ticketTypeResult);
            }
            
            var ticketType = ticketTypeResult.Value!;
            
            if(dueAmount.ContainsKey(ticketType.Currency))
            {
                dueAmount[ticketType.Currency] += newTicket.Quantity * ticketType.Price;
            }
            else
            {
                dueAmount.Add(ticketType.Currency, newTicket.Quantity * ticketType.Price);
            }
        }

        foreach (var resellTicket in cart.ResellTickets)
        {
            var ticketResult = await _ticketService.GetTicketByIdAsync(resellTicket.TicketId);

            if (ticketResult.IsError)
            {
                return Result<Dictionary<string, decimal>>.PropagateError(ticketResult);
            }
            
            var ticket = ticketResult.Value!;

            if (ticket.ResellPrice is not null && ticket.ResellCurrency is not null)
            {
                if (dueAmount.ContainsKey(ticket.ResellCurrency))
                {
                    dueAmount[ticket.ResellCurrency] += ticket.ResellPrice.Value;
                }
                else
                {
                    dueAmount.Add(ticket.ResellCurrency, ticket.ResellPrice.Value);
                }
            }
            else
            {
                if (dueAmount.ContainsKey(ticket.Type.Currency))
                {
                    dueAmount[ticket.Type.Currency] += ticket.Type.Price;
                }
                else
                {
                    dueAmount.Add(ticket.Type.Currency, ticket.Type.Price);
                }
            }
        }
        
        return Result<Dictionary<string, decimal>>.Success(dueAmount);
    }

    public async Task<Result<CheckoutResult>> CheckoutAsync(string customerEmail, decimal amount, string currency,
        string cardNumber, string cardExpiry, string cvv)
    {
        var dueAmountResult = await GetDueAmountAsync(customerEmail);

        if (dueAmountResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(dueAmountResult);
        }

        var currencyExists = dueAmountResult.Value!.TryGetValue(currency, out var dueAmount);

        if (!currencyExists)
        {
            return Result<CheckoutResult>.Failure(StatusCodes.Status400BadRequest,
                $"no tickets paid in {currency} found in cart");
        }
        
        if (dueAmount != amount)
        {
            return Result<CheckoutResult>.Failure(StatusCodes.Status400BadRequest,
                $"the given amount {amount} {currency} is different than the expected amount of {dueAmount} {currency}");
        }

        var paymentResult =
            await _paymentGatewayService.ProcessPayment(new PaymentRequestPG(amount, currency, cardNumber, cardExpiry,
                cvv, false));

        if (paymentResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(paymentResult);
        }
        
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var getCustomerResult = await _customerRepository.GetCustomerByEmailAsync(customerEmail);

        if (getCustomerResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(getCustomerResult);
        }
        
        var owner = getCustomerResult.Value!;
        
        var generateTicketsResult = await GenerateBoughtTicketsAsync(cart, owner, currency);

        if (generateTicketsResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(generateTicketsResult);
        }

        var boughtTickets = generateTicketsResult.Value!;

        var passOwnershipResult = await PassTicketOwnershipAsync(cart, owner, currency);

        if (passOwnershipResult.IsError)
        {
            return Result<CheckoutResult>.PropagateError(passOwnershipResult);
        }

        var resoldTickets = passOwnershipResult.Value!;

        List<Ticket> allTickets = [..boughtTickets, ..resoldTickets];
        
        var payment = paymentResult.Value!;

        return Result<CheckoutResult>.Success(new CheckoutResult(allTickets, payment));
    }

    private async Task<Result<List<Ticket>>> GenerateBoughtTicketsAsync(ShoppingCart cart, Customer owner, string currency)
    {
        var removals = new List<(Guid id, uint amount)>();

        var newTickets = new List<Ticket>();
        
        foreach (var ticket in cart.NewTickets)
        {
            var ticketTypeResult = await _ticketService.GetTicketTypeByIdAsync(ticket.TicketTypeId);

            if (ticketTypeResult.IsError)
            {  
                return Result<List<Ticket>>.PropagateError(ticketTypeResult);
            }
            
            var type = ticketTypeResult.Value!;

            if (type.Currency == currency)
            {
                removals.Add((ticket.TicketTypeId, ticket.Quantity));
                
                for (var i = 0; i < ticket.Quantity; i++)
                {
                    var createTicketResult = await _ticketService.CreateTicketAsync(type, owner);

                    if (createTicketResult.IsError)
                    {
                        return Result<List<Ticket>>.PropagateError(createTicketResult);
                    }
                    
                    newTickets.Add(createTicketResult.Value!);
                }
            }
        }

        foreach (var (id, amount) in removals)
        {
            var removalResult = await RemoveNewTicketsFromCartAsync(id, amount, owner.Email);

            if (removalResult.IsError)
            {
                return Result<List<Ticket>>.PropagateError(removalResult);
            }
        }

        return Result<List<Ticket>>.Success(newTickets);
    }
    
    private async Task<Result<List<Ticket>>> PassTicketOwnershipAsync(ShoppingCart cart, Customer newOwner, string currency)
    {
        var removals = new List<Guid>();

        var resoldTickets = new List<Ticket>();
        
        foreach (var resellTicket in cart.ResellTickets)
        {
            var ticketResult = await _ticketService.GetTicketByIdAsync(resellTicket.TicketId);

            if (ticketResult.IsError)
            {  
                return Result<List<Ticket>>.PropagateError(ticketResult);
            }
            
            var ticket = ticketResult.Value!;

            if ((ticket.ResellCurrency ?? ticket.Type.Currency) == currency)
            {
                removals.Add(ticket.Id);

                var createTicketResult = await _ticketService.ChangeTicketOwnershipViaResellAsync(ticket, newOwner);

                if (createTicketResult.IsError)
                {
                    return Result<List<Ticket>>.PropagateError(createTicketResult);
                }
                
                resoldTickets.Add(ticket);
            }
        }

        foreach (var id in removals)
        {
            var removalResult = await RemoveResellTicketFromCartAsync(id, newOwner.Email);

            if (removalResult.IsError)
            {
                return Result<List<Ticket>>.PropagateError(removalResult);
            }
        }

        return Result<List<Ticket>>.Success(resoldTickets);
    }
}