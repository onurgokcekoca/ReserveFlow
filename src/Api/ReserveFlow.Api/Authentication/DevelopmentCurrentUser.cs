using ReserveFlow.Shared.Application.Authentication;

namespace ReserveFlow.Api.Authentication
{
    internal sealed class DevelopmentCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}
