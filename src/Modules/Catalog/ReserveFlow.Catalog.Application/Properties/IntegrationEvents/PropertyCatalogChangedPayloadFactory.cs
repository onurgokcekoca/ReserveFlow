using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Application.Properties.IntegrationEvents;

public static class PropertyCatalogChangedPayloadFactory
{
    public static PropertyCatalogChangedPayload Create(Property property)
    {
        var roomTypeSnapshots = property.RoomTypes
            .Select(rt => new RoomTypeSnapshotPayload(
                rt.Id,
                rt.Name,
                rt.Capacity))
            .ToArray();

        return new PropertyCatalogChangedPayload(
            property.Id,
            property.Name,
            property.Description,
            property.Location.Country,
            property.Location.City,
            property.Location.AddressLine,
            property.PropertyType.ToString(),
            property.ProviderId,
            roomTypeSnapshots);
    }
}
