using ReserveFlow.Shared.Kernel.Abstractions;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Kernel.ValueObjects;

namespace ReserveFlow.Catalog.Domain.Properties;

public sealed class Property : AggregateRoot<Guid>
{
    private readonly List<RoomType> _roomTypes = [];

    private Property(
        Guid id,
        string name,
        string description,
        Location location,
        PropertyType propertyType,
        Guid providerId)
        : base(id)
    {
        Name = name;
        Description = description;
        Location = location;
        PropertyType = propertyType;
        ProviderId = providerId;
    }

    private Property(
        Guid id,
        string name,
        string description,
        PropertyType propertyType,
        Guid providerId)
        : base(id)
    {
        Name = name;
        Description = description;
        PropertyType = propertyType;
        ProviderId = providerId;
        Location = null!;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Location Location { get; private set; }

    public PropertyType PropertyType { get; private set; }

    public Guid ProviderId { get; private set; }

    public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes;

    public static AppResult<Property> Create(
        string? name,
        string? description,
        Location? location,
        PropertyType propertyType,
        Guid providerId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.NameRequired",
                    "Property name is required."));
        }

        string normalizedName = name.Trim();

        if (normalizedName.Length > 200)
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.NameTooLong",
                    "Property name must not exceed 200 characters."));
        }

        string normalizedDescription = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : description.Trim();

        if (normalizedDescription.Length > 2000)
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.DescriptionTooLong",
                    "Property description must not exceed 2000 characters."));
        }

        if (location is null)
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.LocationRequired",
                    "Property location is required."));
        }

        if (propertyType == PropertyType.Unknown)
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.InvalidType",
                    "A valid property type is required."));
        }

        if (providerId == Guid.Empty)
        {
            return AppResult.Failure<Property>(
                ResultError.Validation(
                    "Property.ProviderRequired",
                    "Property provider is required."));
        }

        Guid id = Guid.CreateVersion7();

        var property = new Property(
            id,
            normalizedName,
            normalizedDescription,
            location,
            propertyType,
            providerId);

        return AppResult.Success(property);
    }

    public AppResult<RoomType> AddRoomType(
        string? name,
        int capacity,
        int totalRooms,
        Money? basePrice)
    {
        string normalizedName = name?.Trim() ?? string.Empty;

        bool roomTypeExists = _roomTypes.Any(
            roomType => string.Equals(
                roomType.Name,
                normalizedName,
                StringComparison.OrdinalIgnoreCase));

        if (roomTypeExists)
        {
            return AppResult.Failure<RoomType>(
                ResultError.Conflict(
                    "Property.DuplicateRoomType",
                    "A room type with the same name already exists in this property."));
        }

        AppResult<RoomType> roomTypeResult = RoomType.Create(
            name,
            capacity,
            totalRooms,
            basePrice);

        if (roomTypeResult.IsFailure)
        {
            return AppResult.Failure<RoomType>(
                roomTypeResult.Error);
        }

        RoomType roomType = roomTypeResult.Value;

        _roomTypes.Add(roomType);

        return AppResult.Success(roomType);
    }
}
