namespace TickAPI.Events.DTOs.Response;

public record GetEventResponsePriceInfoDto(
    decimal Price,
    string Currency
);
