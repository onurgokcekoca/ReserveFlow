using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application.Properties.GetProperty;

public record GetPropertyQuery(Guid PropertyId) : IQuery<AppResult<PropertyDetailsResponse>>;
