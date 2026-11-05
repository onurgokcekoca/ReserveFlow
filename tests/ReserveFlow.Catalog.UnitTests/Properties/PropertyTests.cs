using ReserveFlow.Catalog.Domain.Properties;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Kernel.ValueObjects;

namespace ReserveFlow.Catalog.UnitTests.Properties;

public class PropertyTests
{
    [Fact]
    public void CreateShouldReturnSuccessWhenInputIsValid()
    {
        // Arrange
        var locationResult = Location.Create(
            "Türkiye",
            "Antalya",
            "Kemer Merkez",
            36.602m,
            30.560m);

        var providerId = Guid.CreateVersion7();

        // Act
        var result = Property.Create(
            "ReserveFlow Resort",
            "Beachfront resort.",
            locationResult.Value,
            PropertyType.Resort,
            providerId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("ReserveFlow Resort", result.Value.Name);
        Assert.Equal(providerId, result.Value.ProviderId);
        Assert.Equal(PropertyType.Resort, result.Value.PropertyType);
    }

    [Fact]
    public void CreateShouldReturnFailureWhenNameIsEmpty()
    {
        var locationResult = Location.Create(
            "Türkiye",
            "Antalya",
            "Kemer Merkez",
            36.602m,
            30.560m);

        var providerId = Guid.CreateVersion7();

        var result = Property.Create(
            "",
            "Beachfront resort.",
            locationResult.Value,
            PropertyType.Resort,
            providerId);

        Assert.True(result.IsFailure);
        Assert.Equal("Property.NameRequired", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void CreateShouldReturnFailureWhenPropertyTypeIsUnknown()
    {
        // Arrange
        var locationResult = Location.Create(
            "Türkiye",
            "Antalya",
            "Kemer Merkez",
            36.602m,
            30.560m);

        var providerId = Guid.CreateVersion7();

        var result = Property.Create(
            "ReserveFlow Resort",
            "Beachfront resort.",
            locationResult.Value,
            PropertyType.Unknown,
            providerId);

        Assert.True(result.IsFailure);
        Assert.Equal("Property.InvalidType", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void AddRoomTypeShouldAddRoomTypeWhenInputIsValid()
    {
        // Arrange
        var locationResult = Location.Create(
            "Türkiye",
            "Antalya",
            "Kemer Merkez",
            36.602m,
            30.560m);

        var providerId = Guid.CreateVersion7();

        var propertyResult = Property.Create(
            "ReserveFlow Resort",
            "Beachfront resort.",
            locationResult.Value,
            PropertyType.Resort,
            providerId);

        var moneyResult = Money.Create(
            4500m,
            "TRY");

        var property = propertyResult.Value;
        var basePrice = moneyResult.Value;

        // Act
        var result = property.AddRoomType(
            "Deluxe Sea View",
            2,
            5,
            basePrice);

        // Assert
        Assert.True(result.IsSuccess);

        var roomType = Assert.Single(property.RoomTypes);

        Assert.Equal("Deluxe Sea View", roomType.Name);
        Assert.Equal(2, roomType.Capacity);
        Assert.Equal(5, roomType.TotalRooms);
        Assert.Equal(4500m, roomType.BasePrice.Amount);
        Assert.Equal("TRY", roomType.BasePrice.Currency);
    }

    [Fact]
    public void AddRoomTypeShouldReturnConflictWhenRoomTypeNameAlreadyExists()
    {
        // Arrange
        var locationResult = Location.Create(
            "Türkiye",
            "Antalya",
            "Kemer Merkez",
            36.602m,
            30.560m);

        var providerId = Guid.CreateVersion7();

        var propertyResult = Property.Create(
            "ReserveFlow Resort",
            "Beachfront resort.",
            locationResult.Value,
            PropertyType.Resort,
            providerId);

        var moneyResult = Money.Create(
            4500m,
            "TRY");

        var property = propertyResult.Value;
        var basePrice = moneyResult.Value;

        // Act
        var result = property.AddRoomType(
            "Deluxe Sea View",
            2,
            5,
            basePrice);

        var result2 = property.AddRoomType(
           "     Deluxe Sea View     ",
           2,
           5,
           basePrice);

        // Assert
        Assert.True(result2.IsFailure);
        Assert.Equal("Property.DuplicateRoomType", result2.Error.Code);
        Assert.Equal(ErrorType.Conflict, result2.Error.Type);
        Assert.Single(property.RoomTypes);
    }
}
