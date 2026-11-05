using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Inventory.Application.Inventories.GetRoomAvailability
{
    public class GetRoomAvailabilityQueryHandler : IQueryHandler<GetRoomAvailabilityQuery, AppResult<RoomAvailabilityResponse>>
    {
        private readonly IRoomInventoryReadRepository _roomInventoryReadRepository;

        public GetRoomAvailabilityQueryHandler(IRoomInventoryReadRepository roomInventoryReadRepository)
        {
            _roomInventoryReadRepository = roomInventoryReadRepository;
        }

        public async Task<AppResult<RoomAvailabilityResponse>> HandleAsync(GetRoomAvailabilityQuery query, CancellationToken cancellationToken)
        {
            if (query.checkIn > query.checkOut)
            {
                return AppResult.Failure<RoomAvailabilityResponse>(ResultError.Validation("Error", "Checkin date can not be before checkout date"));
            }

            if (query.checkIn == query.checkOut)
            {
                return AppResult.Failure<RoomAvailabilityResponse>(ResultError.Validation("Error", "Checkin date can not be equal checkout date"));
            }

            if (query.rooms < 1)
            {
                return AppResult.Failure<RoomAvailabilityResponse>(ResultError.Validation("Error", "Rooms must be greater than 0"));
            }

            var inventory = await _roomInventoryReadRepository.GetByRoomTypeAndDateRangeAsync(query.roomTypeId, query.checkIn, query.checkOut, cancellationToken);

            var expectedNights = query.checkOut.DayNumber - query.checkIn.DayNumber;

            var hasCompleteInventory = inventory.Count == expectedNights;

            var maxAvailableRooms = hasCompleteInventory ? inventory.Min(ri => ri.AvailableRooms) : 0;

            var response = new RoomAvailabilityResponse
            {
                RoomTypeId = query.roomTypeId,
                CheckIn = query.checkIn,
                CheckOut = query.checkOut,
                RequestedRooms = query.rooms,
                MaxAvailableRooms = maxAvailableRooms,
                IsAvailable = hasCompleteInventory && maxAvailableRooms >= query.rooms
            };

            return AppResult.Success(response);
        }
    }
}
