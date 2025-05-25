namespace TickAPI.Events.DTOs.Request;

public record EventFiltersDto(
    string? SearchQuery,
    DateTime? StartDate,
    DateTime? MinStartDate,
    DateTime? MaxStartDate,
    DateTime? EndDate,
    DateTime? MinEndDate,
    DateTime? MaxEndDate,
    decimal? MinPrice,
    decimal? MaxPrice,
    uint? MinAge,
    uint? MaxMinimumAge,
    string? AddressCountry,
    string? AddressCity,
    string? AddressStreet,
    uint? HouseNumber,
    uint? FlatNumber,
    string? PostalCode,
    List<string>? CategoriesNames
);
