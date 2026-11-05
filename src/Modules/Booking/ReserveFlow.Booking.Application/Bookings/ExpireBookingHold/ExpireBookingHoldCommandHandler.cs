using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.ExpireBookingHold
{
    public class ExpireBookingHoldCommandHandler : ICommandHandler<ExpireBookingHoldCommand, AppResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IInventoryReservationGateway _inventoryReservationGateway;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;

        public ExpireBookingHoldCommandHandler(IBookingRepository bookingRepository, IInventoryReservationGateway inventoryReservationGateway, IBookingUnitOfWork bookingUnitOfWork)
        {
            _bookingRepository = bookingRepository;
            _inventoryReservationGateway = inventoryReservationGateway;
            _bookingUnitOfWork = bookingUnitOfWork;
        }

        public async Task<AppResult> HandleAsync(ExpireBookingHoldCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.bookingId, cancellationToken);

            if (booking is null)
            {
                return AppResult.Failure(ResultError.NotFound("BookingHold.NotFound", "Booking not found."));
            }

            var utcNow = DateTime.UtcNow;

            var canExpire = booking.CanExpire(utcNow);

            if (!canExpire)
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.ExpiresAtUtc", "ExpiresAtUtc has not yet passed."));
            }

            var releaseResult = await _inventoryReservationGateway.ReleaseAsync(booking.RoomTypeId, booking.CheckIn, booking.CheckOut, booking.Rooms, cancellationToken);

            if (releaseResult.IsFailure)
            {
                return AppResult.Failure(releaseResult.Error);
            }

            var expireResult = booking.Expire(utcNow);

            if (!expireResult.IsSuccess)
            {
                return AppResult.Failure(expireResult.Error);
            }

            try
            {
                await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                var compensationResult = await _inventoryReservationGateway.ReserveAsync(
                    booking.RoomTypeId,
                    booking.CheckIn,
                    booking.CheckOut,
                    booking.Rooms,
                    cancellationToken);

                throw;
            }

            return AppResult.Success();
        }
    }
}
