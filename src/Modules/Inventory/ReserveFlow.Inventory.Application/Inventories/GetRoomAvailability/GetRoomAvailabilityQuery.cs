using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.GetRoomAvailability;

public sealed record GetRoomAvailabilityQuery(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms) : IQuery<AppResult<RoomAvailabilityResponse>>;
