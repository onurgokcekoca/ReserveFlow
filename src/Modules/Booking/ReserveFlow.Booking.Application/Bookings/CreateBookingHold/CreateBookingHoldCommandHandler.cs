using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Booking.Domain.Bookings;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.CreateBookingHold
{
    public class CreateBookingHoldCommandHandler : ICommandHandler<CreateBookingHoldCommand, AppResult<Guid>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IInventoryReservationGateway _inventoryReservationGateway;

        public CreateBookingHoldCommandHandler(IBookingRepository bookingRepository, IBookingUnitOfWork bookingUnitOfWork, IInventoryReservationGateway inventoryReservationGateway)
        {
            _bookingRepository = bookingRepository;
            _bookingUnitOfWork = bookingUnitOfWork;
            _inventoryReservationGateway = inventoryReservationGateway;
        }

        public async Task<AppResult<Guid>> HandleAsync(CreateBookingHoldCommand command, CancellationToken cancellationToken)
        {
            if (command.checkIn >= command.checkOut)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("BookingHold.CheckIn", "Check-in date must be before check-out date."));
            }

            if (command.rooms <= 0)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("BookingHold.Rooms", "Number of rooms must be greater than zero."));
            }

            if (command.guests <= 0)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("BookingHold.Guests", "Number of guests must be greater than zero."));
            }

            if (command.propertyId == Guid.Empty)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("BookingHold.PropertyId", "Property ID must be a valid GUID."));
            }

            if (command.roomTypeId == Guid.Empty)
            {
                return AppResult.Failure<Guid>(ResultError.Validation("BookingHold.RoomTypeId", "Room type ID must be a valid GUID."));
            }

            var createdAtUtc = DateTime.UtcNow;
            var expiresAtUtc = createdAtUtc.AddMinutes(10);

            var bookingHoldResult = BookingHold.Create(command.propertyId, command.roomTypeId, command.checkIn, command.checkOut, command.rooms, command.guests, createdAtUtc, expiresAtUtc);

            if (!bookingHoldResult.IsSuccess) return AppResult.Failure<Guid>(bookingHoldResult.Error);

            var inventoryReservationResult = await _inventoryReservationGateway.ReserveAsync(command.roomTypeId, command.checkIn, command.checkOut, command.rooms, cancellationToken);

            if (!inventoryReservationResult.IsSuccess)
            {
                return AppResult.Failure<Guid>(inventoryReservationResult.Error);
            }

            try
            {
                await _bookingRepository.AddAsync(bookingHoldResult.Value, cancellationToken);
                await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);
                return AppResult.Success(bookingHoldResult.Value.Id);
            }
            catch
            {
                var compensationResult = await _inventoryReservationGateway.ReleaseAsync(command.roomTypeId, command.checkIn, command.checkOut, command.rooms, cancellationToken);
                throw;
            }
        }
    }
}
