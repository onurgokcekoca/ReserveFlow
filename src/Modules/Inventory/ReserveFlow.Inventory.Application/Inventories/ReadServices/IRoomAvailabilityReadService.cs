namespace ReserveFlow.Inventory.Application.Inventories.ReadServices
{
    public interface IRoomAvailabilityReadService
    {
        Task<IReadOnlyCollection<RoomTypeAvailabilityReadModel>> GetAvailabilityAsync(IReadOnlyCollection<Guid> roomTypeIds, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken);
    }
}
