namespace ReserveFlow.Shared.Kernel.Abstractions
{
    public class Entity<TId> where TId : notnull
    {
        protected Entity(TId id)
        {
            Id = id;
        }

        public TId Id { get; protected set; }
    }
}
