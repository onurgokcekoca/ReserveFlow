namespace ReserveFlow.Shared.Application.Authentication
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
    }
}
