using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Booking.Domain.Bookings;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.CancelledBookingHold
{
    public class CancelledBookingHoldCommandHandler : ICommandHandler<CancelledBookingHoldCommand, AppResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IInventoryReservationGateway _inventoryReservationGateway;

        public CancelledBookingHoldCommandHandler(IBookingRepository bookingRepository, IBookingUnitOfWork bookingUnitOfWork, IInventoryReservationGateway inventoryReservationGateway)
        {
            _bookingRepository = bookingRepository;
            _bookingUnitOfWork = bookingUnitOfWork;
            _inventoryReservationGateway = inventoryReservationGateway;
        }

        public async Task<AppResult> HandleAsync(CancelledBookingHoldCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.bookingId, cancellationToken);

            if (booking is null)
            {
                return AppResult.Failure(ResultError.NotFound("BookingHold.NotFound", $"Booking hold with ID {command.bookingId} not found."));
            }

            if (booking.Status == StatusType.Cancelled)
            {
                return AppResult.Success();
            }

            if (booking.Status == StatusType.Expired)
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Expired", $"Booking hold with ID {command.bookingId} is expired."));
            }

            var currentUtc = DateTime.UtcNow;

            if (booking.CanExpire(currentUtc))
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Expired", $"Booking hold with ID {command.bookingId} is expired."));
            }

            if (booking.HasReservationStarted(currentUtc))
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Confirmed", $"Booking hold with ID {command.bookingId} is confirmed and cannot be cancelled after check-in date."));
            }

            var reselaseResult = await _inventoryReservationGateway.ReleaseAsync(booking.RoomTypeId, booking.CheckIn, booking.CheckOut, booking.Rooms, cancellationToken);

            if (reselaseResult.IsFailure)
            {
                return AppResult.Failure(reselaseResult.Error);
            }

            var cancelResult = booking.Cancel(currentUtc);

            if (cancelResult.IsFailure)
            {
                var reserveResult = await _inventoryReservationGateway.ReserveAsync(booking.RoomTypeId, booking.CheckIn, booking.CheckOut, booking.Rooms, cancellationToken);
                if (reserveResult.IsFailure)
                {
                    return AppResult.Failure(reserveResult.Error);
                }
                return AppResult.Failure(cancelResult.Error);
            }

            try
            {
                await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);
                return AppResult.Success();
            }
            catch
            {
                await _inventoryReservationGateway.ReserveAsync(booking.RoomTypeId, booking.CheckIn, booking.CheckOut, booking.Rooms, cancellationToken);

                throw;
            }
        }
    }
}
