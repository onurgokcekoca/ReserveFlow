using ReserveFlow.Catalog.Application.Properties.ReadServices;
using ReserveFlow.Inventory.Application.Inventories.ReadServices;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Search.Application.AvailableRoomTypes
{
    internal sealed class SearchAvailableRoomTypesQueryHandler(IPropertyRoomTypeReadService propertyReader, IRoomAvailabilityReadService availabilityReader) : IQueryHandler<SearchAvailableRoomTypesQuery, AppResult<SearchAvailableRoomTypesResponse>>
    {
        public async Task<AppResult<SearchAvailableRoomTypesResponse>> HandleAsync(SearchAvailableRoomTypesQuery query, CancellationToken cancellationToken)
        {
            if (query.CheckIn >= query.CheckOut)
            {
                return AppResult.Failure<SearchAvailableRoomTypesResponse>(ResultError.Validation("Search.InvalidDateRange", "Check-in date must be before check-out date."));
            }

            if (query.Rooms <= 0)
            {
                return AppResult.Failure<SearchAvailableRoomTypesResponse>(ResultError.Validation("Search.InvalidRooms", "Requested rooms must be greater than zero."));
            }

            if (query.Guests <= 0)
            {
                return AppResult.Failure<SearchAvailableRoomTypesResponse>(ResultError.Validation("Search.InvalidGuests", "Guests must be greater than zero."));
            }

            var property = await propertyReader.GetByPropertyIdAsync(query.PropertyId, cancellationToken);

            if (property is null)
            {
                return AppResult.Failure<SearchAvailableRoomTypesResponse>(ResultError.NotFound("Search.PropertyNotFound", "Property was not found."));
            }

            var capacityEligibleRoomTypes = property.RoomTypes.Where(rt => rt.Capacity * query.Rooms >= query.Guests).ToList();

            if (capacityEligibleRoomTypes.Count == 0)
            {
                return AppResult.Success(new SearchAvailableRoomTypesResponse
                {
                    PropertyId = query.PropertyId,
                    CheckIn = query.CheckIn,
                    CheckOut = query.CheckOut,
                    RequestedRooms = query.Rooms,
                    Guests = query.Guests,
                    RoomTypes = []
                });
            }

            var roomTypeIds = capacityEligibleRoomTypes.Select(rt => rt.RoomTypeId).ToArray();

            var availabilities = await availabilityReader.GetAvailabilityAsync(
                    roomTypeIds,
                    query.CheckIn,
                    query.CheckOut,
                    cancellationToken);

            var availabilityByRoomType = availabilities.ToDictionary(x => x.RoomTypeId);

            var availableRoomTypes = capacityEligibleRoomTypes
                .Where(roomType => availabilityByRoomType.TryGetValue(roomType.RoomTypeId, out var availability) && availability.HasCompleteInventory && availability.MaxAvailableRooms >= query.Rooms)
                .Select(roomType =>
                {
                    var availability = availabilityByRoomType[roomType.RoomTypeId];

                    return new AvailableRoomTypeResponse
                    {
                        RoomTypeId = roomType.RoomTypeId,
                        Name = roomType.Name,
                        Capacity = roomType.Capacity,
                        BasePrice = roomType.BasePrice,
                        Currency = roomType.Currency,
                        MaxAvailableRooms = availability.MaxAvailableRooms
                    };
                })
                .ToList();

            return AppResult.Success(new SearchAvailableRoomTypesResponse
            {
                PropertyId = query.PropertyId,
                CheckIn = query.CheckIn,
                CheckOut = query.CheckOut,
                RequestedRooms = query.Rooms,
                Guests = query.Guests,
                RoomTypes = availableRoomTypes
            });
        }
    }
}
