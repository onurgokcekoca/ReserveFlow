namespace ReserveFlow.Inventory.Application.Exceptions
{
    public class InventoryConcurrencyException : Exception
    {
        public InventoryConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InventoryConcurrencyException() : base()
        {
        }

        public InventoryConcurrencyException(string? message) : base(message)
        {
        }
    }
}
