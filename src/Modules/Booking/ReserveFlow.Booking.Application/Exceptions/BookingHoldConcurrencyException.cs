namespace ReserveFlow.Booking.Application.Exceptions
{
    public class BookingHoldConcurrencyException : Exception
    {
        public BookingHoldConcurrencyException() : base()
        {
        }

        public BookingHoldConcurrencyException(string? message) : base(message)
        {
        }

        public BookingHoldConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
