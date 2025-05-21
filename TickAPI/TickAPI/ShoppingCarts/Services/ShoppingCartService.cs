using TickAPI.Common.Payment.Abstractions;
using TickAPI.Common.Payment.Models;
using TickAPI.Common.Results;
using TickAPI.Common.Results.Generic;
using TickAPI.Events.Models;
using TickAPI.ShoppingCarts.Abstractions;
using TickAPI.ShoppingCarts.DTOs.Response;
using TickAPI.ShoppingCarts.Mappers;
using TickAPI.Tickets.Abstractions;
using TickAPI.Tickets.Models;

namespace TickAPI.ShoppingCarts.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly ITicketService _ticketService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, ITicketService ticketService,
        IPaymentGatewayService paymentGatewayService)
    {
        _shoppingCartRepository = shoppingCartRepository;
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

    public async Task<Result<GetShoppingCartTicketsResponseDto>> GetTicketsFromCartAsync(string customerEmail)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<GetShoppingCartTicketsResponseDto>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;
        
        var newTickets = new List<GetShoppingCartTicketsNewTicketDetailsResponseDto>();

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
        
        // TODO: Add resell ticket parsing
        
        var result = new GetShoppingCartTicketsResponseDto(newTickets, []);
        
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

    public async Task<Result<decimal>> GetDueAmountAsync(string customerEmail, string currency)
    {
        var getShoppingCartResult = await _shoppingCartRepository.GetShoppingCartByEmailAsync(customerEmail);

        if (getShoppingCartResult.IsError)
        {
            return Result<decimal>.PropagateError(getShoppingCartResult);
        }
        
        var cart = getShoppingCartResult.Value!;

        decimal total = 0;

        foreach (var newTicket in cart.NewTickets)
        {
            var ticketTypeResult = await _ticketService.GetTicketTypeByIdAsync(newTicket.TicketTypeId);

            if (ticketTypeResult.IsError)
            {
                return Result<decimal>.PropagateError(ticketTypeResult);
            }
            
            var ticketType = ticketTypeResult.Value!;

            if (ticketType.Currency == currency)
            {
                total += newTicket.Quantity * ticketType.Price;
            }
        }
        
        // TODO: Add resell tickets to the calculations
        
        return Result<decimal>.Success(total);
    }

    public async Task<Result<PaymentResponsePG>> CheckoutAsync(string customerEmail, decimal amount, string currency,
        string cardNumber, string cardExpiry, string cvv)
    {
        var dueAmountResult = await GetDueAmountAsync(customerEmail, currency);

        if (dueAmountResult.IsError)
        {
            return Result<PaymentResponsePG>.PropagateError(dueAmountResult);
        }

        var dueAmount = dueAmountResult.Value;

        if (dueAmount != amount)
        {
            return Result<PaymentResponsePG>.Failure(StatusCodes.Status400BadRequest,
                $"the given amount {amount} {currency} is different than the expected amount of {dueAmount} {currency}");
        }

        var paymentResult =
            await _paymentGatewayService.ProcessPayment(new PaymentRequestPG(amount, currency, cardNumber, cardExpiry,
                cvv, false));

        if (paymentResult.IsError)
        {
            return Result<PaymentResponsePG>.PropagateError(paymentResult);
        }

        var payment = paymentResult.Value!;

        return Result<PaymentResponsePG>.Success(payment);
    }
}