using Microsoft.Extensions.Options;
using TickAPI.Common.Redis.Abstractions;
using TickAPI.ShoppingCarts.Models;
using TickAPI.ShoppingCarts.Options;

namespace TickAPI.ShoppingCarts.Background;

public class ShoppingCartSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShoppingCartSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval;

    public ShoppingCartSyncBackgroundService(IServiceProvider serviceProvider,
        ILogger<ShoppingCartSyncBackgroundService> logger, IOptions<ShoppingCartOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _syncInterval = TimeSpan.FromMinutes(options.Value.SyncIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
                await SyncTicketTypeCountersAsync(redisService, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while syncing shopping cart ticket counters");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task SyncTicketTypeCountersAsync(IRedisService redisService, CancellationToken cancellationToken)
    {
        var cartKeys = await redisService.GetKeysByPatternAsync("cart:*");
        var ticketTypeCounts = new Dictionary<Guid, long>();

        foreach (var cartKey in cartKeys)
        {
            var cart = await redisService.GetObjectAsync<ShoppingCart>(cartKey);
            if (cart == null) continue;

            foreach (var ticket in cart.NewTickets)
            {
                if (ticketTypeCounts.ContainsKey(ticket.TicketTypeId))
                    ticketTypeCounts[ticket.TicketTypeId] += ticket.Quantity;
                else
                    ticketTypeCounts[ticket.TicketTypeId] = ticket.Quantity;
            }

            if (cancellationToken.IsCancellationRequested) return;
        }
        
        foreach (var kvp in ticketTypeCounts)
        {
            await redisService.SetLongValueAsync($"amount:{kvp.Key}", kvp.Value);
        }
        
        var existingAmountKeys = await redisService.GetKeysByPatternAsync("amount:*");

        foreach (var key in existingAmountKeys)
        {
            var typeIdStr = key.Split(":").Last();
            if (!Guid.TryParse(typeIdStr, out var ticketTypeId)) continue;

            if (!ticketTypeCounts.ContainsKey(ticketTypeId) || ticketTypeCounts[ticketTypeId] == 0)
            {
                await redisService.DeleteKeyAsync(key);
            }

            if (cancellationToken.IsCancellationRequested) return;
        }

        _logger.LogInformation("Synchronized ticket counters for {Count} ticket types", ticketTypeCounts.Count);
    }
}
