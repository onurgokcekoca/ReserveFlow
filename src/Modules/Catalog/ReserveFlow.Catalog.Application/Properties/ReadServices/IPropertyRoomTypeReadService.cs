namespace ReserveFlow.Catalog.Application.Properties.ReadServices
{
    public interface IPropertyRoomTypeReadService
    {
        Task<PropertyRoomTypesReadModel?> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken);
    }
}
