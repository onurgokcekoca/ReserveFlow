using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application.Properties.AddRoomType;

public record AddRoomTypeCommand(Guid PropertyId, string? Name, int Capacity, int TotalRooms, decimal Amount, string? Currency) : ICommand<AppResult<Guid>> { };
