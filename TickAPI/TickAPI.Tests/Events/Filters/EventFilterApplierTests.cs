using Moq;
using TickAPI.Events.Abstractions;
using TickAPI.Events.DTOs.Request;
using TickAPI.Events.Models;
using TickAPI.Events.Filters;

namespace TickAPI.Tests.Events.Filters;

public class EventFilterApplierTests
{
    private readonly Mock<IEventFilter> _mockEventFilter;
    private readonly EventFilterApplier _eventFilterApplier;
    private readonly IQueryable<Event> _emptyQueryable = new List<Event>().AsQueryable();

    public EventFilterApplierTests()
    {
        _mockEventFilter = new Mock<IEventFilter>();
        _mockEventFilter.Setup(ef => ef.GetEvents()).Returns(_emptyQueryable);
        _eventFilterApplier = new EventFilterApplier(_mockEventFilter.Object);
    }

    [Fact]
    public void ApplyFilters_WithName_ShouldCallFilterByName()
    {
        // Arrange
        var filters = new EventFiltersDto(
            SearchQuery: "test event",
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByName(filters.SearchQuery!), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByDescription(filters.SearchQuery!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }
    
    [Fact]
    public void ApplyFilters_WithStartDate_ShouldCallFilterByStartDate()
    {
        // Arrange
        var startDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: startDate,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByStartDate(filters.StartDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMinStartDate_ShouldCallFilterByMinStartDate()
    {
        // Arrange
        var minStartDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: minStartDate,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMinStartDate(filters.MinStartDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMaxStartDate_ShouldCallFilterByMaxStartDate()
    {
        // Arrange
        var maxStartDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: maxStartDate,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMaxStartDate(filters.MaxStartDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithEndDate_ShouldCallFilterByEndDate()
    {
        // Arrange
        var endDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: endDate,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByEndDate(filters.EndDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMinEndDate_ShouldCallFilterByMinEndDate()
    {
        // Arrange
        var minEndDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: minEndDate,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMinEndDate(filters.MinEndDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMaxEndDate_ShouldCallFilterByMaxEndDate()
    {
        // Arrange
        var maxEndDate = new DateTime(2025, 5, 1);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: maxEndDate,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMaxEndDate(filters.MaxEndDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMinPrice_ShouldCallFilterByMinPrice()
    {
        // Arrange
        decimal minPrice = 100;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: minPrice,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMinPrice(filters.MinPrice!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMaxPrice_ShouldCallFilterByMaxPrice()
    {
        // Arrange
        decimal maxPrice = 200;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: maxPrice,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMaxPrice(filters.MaxPrice!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMinAge_ShouldCallFilterByMinAge()
    {
        // Arrange
        uint minAge = 18;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: minAge,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMinAge(filters.MinAge!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMaxMinimumAge_ShouldCallFilterByMaxMinimumAge()
    {
        // Arrange
        uint maxMinimumAge = 21;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: maxMinimumAge,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByMaxMinimumAge(filters.MaxMinimumAge!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithAddressCountry_ShouldCallFilterByAddressCountry()
    {
        // Arrange
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: "Poland",
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressCountry(filters.AddressCountry!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithAddressCity_ShouldCallFilterByAddressCity()
    {
        // Arrange
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: "Warsaw",
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressCity(filters.AddressCity!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithAddressStreetOnly_ShouldCallFilterByAddressStreet()
    {
        // Arrange
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: "Marszałkowska",
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressStreet(filters.AddressStreet!, null, null), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithAddressStreetAndHouseNumber_ShouldCallFilterByAddressStreet()
    {
        // Arrange
        uint houseNumber = 12;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: "Marszałkowska",
            HouseNumber: houseNumber,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressStreet(filters.AddressStreet!, filters.HouseNumber, null), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithCompleteAddress_ShouldCallFilterByAddressStreet()
    {
        // Arrange
        uint houseNumber = 12;
        uint flatNumber = 5;
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: "Marszałkowska",
            HouseNumber: houseNumber,
            FlatNumber: flatNumber,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressStreet(filters.AddressStreet!, filters.HouseNumber, filters.FlatNumber), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }
    
    [Fact]
    public void ApplyFilters_WithPostalCode_ShouldCallFilterByAddressPostalCode()
    {
        // Arrange
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: "00-001",
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByAddressPostalCode(filters.PostalCode!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithCategoriesNames_ShouldCallFilterByCategoriesNames()
    {
        // Arrange
        var categoriesNames = new List<string> { "Concert", "Festival", "Exhibition" };
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: categoriesNames
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByCategoriesNames(filters.CategoriesNames!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithMultipleFilters_ShouldCallAllRelevantFilters()
    {
        // Arrange
        var startDate = new DateTime(2025, 5, 1);
        decimal minPrice = 50;
        decimal maxPrice = 200;
        var filters = new EventFiltersDto(
            SearchQuery: "Concert",
            StartDate: startDate,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: "Poland",
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByName(filters.SearchQuery!), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByDescription(filters.SearchQuery!), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByStartDate(filters.StartDate!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByMinPrice(filters.MinPrice!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByMaxPrice(filters.MaxPrice!.Value), Times.Once);
        _mockEventFilter.Verify(ef => ef.FilterByAddressCountry(filters.AddressCountry!), Times.Once);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
    }

    [Fact]
    public void ApplyFilters_WithNoFilters_ShouldOnlyCallGetEvents()
    {
        // Arrange
        var expectedResult = new List<Event> 
        { 
            new Event { Name = "Test Event" } 
        }.AsQueryable();
        _mockEventFilter.Setup(ef => ef.GetEvents()).Returns(expectedResult);
        var filters = new EventFiltersDto(
            SearchQuery: null,
            StartDate: null,
            MinStartDate: null,
            MaxStartDate: null,
            EndDate: null,
            MinEndDate: null,
            MaxEndDate: null,
            MinPrice: null,
            MaxPrice: null,
            MinAge: null,
            MaxMinimumAge: null,
            AddressCountry: null,
            AddressCity: null,
            AddressStreet: null,
            HouseNumber: null,
            FlatNumber: null,
            PostalCode: null,
            CategoriesNames: null
        );

        // Act
        var result = _eventFilterApplier.ApplyFilters(filters);

        // Assert
        _mockEventFilter.Verify(ef => ef.FilterByName(It.IsAny<string>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByDescription(It.IsAny<string>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByStartDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMinStartDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMaxStartDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByEndDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMinEndDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMaxEndDate(It.IsAny<DateTime>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMinPrice(It.IsAny<decimal>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMaxPrice(It.IsAny<decimal>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMinAge(It.IsAny<uint>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByMaxMinimumAge(It.IsAny<uint>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByAddressCountry(It.IsAny<string>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByAddressCity(It.IsAny<string>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.FilterByAddressStreet(It.IsAny<string>(), It.IsAny<uint?>(), It.IsAny<uint?>()), Times.Never);
        _mockEventFilter.Verify(ef => ef.GetEvents(), Times.Once);
        Assert.Same(expectedResult, result);
    }
}