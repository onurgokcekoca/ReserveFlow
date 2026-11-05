using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Search.Application.AvailableRoomTypes
{
    public sealed record SearchAvailableRoomTypesQuery(
        Guid PropertyId,
        DateOnly CheckIn,
        DateOnly CheckOut,
        int Rooms,
        int Guests) : IQuery<AppResult<SearchAvailableRoomTypesResponse>>;
}
