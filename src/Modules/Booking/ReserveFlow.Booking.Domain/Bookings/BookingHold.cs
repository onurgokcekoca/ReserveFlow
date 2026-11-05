using ReserveFlow.Shared.Kernel.Abstractions;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Booking.Domain.Bookings
{
    public sealed class BookingHold : AggregateRoot<Guid>
    {
        private BookingHold(Guid id, Guid propertyId, Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, int guests, StatusType status, DateTime createdAtUtc, DateTime expiresAtUtc) : base(id)
        {
            PropertyId = propertyId;
            RoomTypeId = roomTypeId;
            CheckIn = checkIn;
            CheckOut = checkOut;
            Rooms = rooms;
            Guests = guests;
            Status = status;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
        }

        public Guid PropertyId { get; }
        public Guid RoomTypeId { get; }
        public DateOnly CheckIn { get; }
        public DateOnly CheckOut { get; }
        public int Rooms { get; }
        public int Guests { get; }
        public StatusType Status { get; private set; } = StatusType.Active;
        public DateTime CreatedAtUtc { get; }
        public DateTime ExpiresAtUtc { get; }
        public int Version { get; private set; } = 1;

        public static AppResult<BookingHold> Create(Guid propertyId, Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms, int guests, DateTime createdAtUtc, DateTime expiresAtUtc)
        {
            if (roomTypeId == Guid.Empty)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.RoomTypeId", "Room type ID is required."));
            }

            if (propertyId == Guid.Empty)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.PropertyId", "Property ID is required."));
            }

            if (checkIn >= checkOut)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.CheckIn", "Check-in date must be before check-out date."));
            }

            if (rooms < 0)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.Rooms", "Number of rooms must be a positive integer."));
            }

            if (guests < 1)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.Guests", "Number of guests must be a positive integer."));
            }

            if (createdAtUtc >= expiresAtUtc)
            {
                return AppResult.Failure<BookingHold>(ResultError.Validation("BookingHold.ExpiresAtUtc", "Expiration date must be after creation date."));
            }

            var bookingHold = new BookingHold(Guid.CreateVersion7(), propertyId, roomTypeId, checkIn, checkOut, rooms, guests, StatusType.Active, createdAtUtc, expiresAtUtc);
            return AppResult.Success(bookingHold);
        }

        public AppResult Expire(DateTime currentUtc)
        {
            if (Status != StatusType.Active)
            {
                return AppResult.Failure(ResultError.Validation("BookingHold.Status", "Only active booking holds can be expired."));
            }
            if (currentUtc < ExpiresAtUtc)
            {
                return AppResult.Failure(ResultError.Validation("BookingHold.ExpiresAtUtc", "Cannot expire a booking hold before its expiration date."));
            }

            Version++;
            Status = StatusType.Expired;
            return AppResult.Success();
        }

        public bool CanExpire(DateTime currentUtc)
        {
            return Status == StatusType.Active && currentUtc >= ExpiresAtUtc;
        }

        public bool HasReservationStarted(DateTime currentUtc)
        {
            var currentDate = DateOnly.FromDateTime(currentUtc);

            return Status == StatusType.Confirmed && CheckIn <= currentDate;
        }

        public AppResult Confirm(DateTime currentUtc)
        {
            if (Status == StatusType.Confirmed)
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Status", "Booking hold is already confirmed."));
            }

            if (Status != StatusType.Active)
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Status", "Only active booking holds can be confirmed."));
            }

            if (CanExpire(currentUtc))
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.ExpiresAtUtc", "Cannot confirm an expired booking hold."));
            }

            Status = StatusType.Confirmed;
            Version++;

            return AppResult.Success();
        }

        public AppResult Cancel(DateTime currentUtc)
        {
            if (Status == StatusType.Cancelled)
            {
                return AppResult.Success();
            }

            if (Status == StatusType.Expired)
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.Status", "Booking hold is already expired."));
            }

            if (CanExpire(currentUtc))
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.ExpiresAtUtc", "Cannot cancel an expired booking hold."));
            }

            if (HasReservationStarted(currentUtc))
            {
                return AppResult.Failure(ResultError.Conflict("BookingHold.CheckIn", "Cannot cancel a booking hold on or after the check-in date."));
            }

            Status = StatusType.Cancelled;
            Version++;

            return AppResult.Success();
        }
    }
}
