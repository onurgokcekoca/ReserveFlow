using ReserveFlow.Booking.Application.Abstractions;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Application.Bookings.ConfirmBookingHold
{
    public class ConfirmBookingHoldCommandHandler : ICommandHandler<ConfirmBookingHoldCommand, AppResult>
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IBookingRepository _bookingRepository;
        private readonly IOutboxWriter _outboxWriter;

        public ConfirmBookingHoldCommandHandler(IBookingUnitOfWork bookingUnitOfWork, IBookingRepository bookingRepository, IOutboxWriter outboxWriter)
        {
            _bookingUnitOfWork = bookingUnitOfWork;
            _bookingRepository = bookingRepository;
            _outboxWriter = outboxWriter;
        }

        public async Task<AppResult> HandleAsync(ConfirmBookingHoldCommand command, CancellationToken cancellationToken)
        {
            var bookingHold = await _bookingRepository.GetByIdAsync(command.BookingHoldId, cancellationToken);

            if (bookingHold is null)
            {
                return AppResult.Failure(ResultError.NotFound("BookingHold.NotFound", "Booking hold not found."));
            }

            var currentUtc = DateTime.UtcNow;

            var confirmResult = bookingHold.Confirm(currentUtc);

            if (confirmResult.IsFailure)
            {
                return AppResult.Failure(confirmResult.Error);
            }

            var outboxMessagesPayloadResult = new BookingConfirmedPayload(bookingHold.Id, bookingHold.PropertyId, bookingHold.RoomTypeId, bookingHold.CheckIn, bookingHold.CheckOut, bookingHold.Rooms, bookingHold.Guests);

            await _outboxWriter.WriteAsync("BookingConfirmed", outboxMessagesPayloadResult, currentUtc, cancellationToken);

            await _bookingUnitOfWork.SaveChangesAsync(cancellationToken);

            return AppResult.Success();
        }
    }
}
