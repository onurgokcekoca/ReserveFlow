using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Application.Abstractions.Caching;
using ReserveFlow.Catalog.Application.Properties.IntegrationEvents;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;
using ReserveFlow.Shared.Kernel.ValueObjects;

namespace ReserveFlow.Catalog.Application.Properties.AddRoomType;

internal sealed class AddRoomTypeCommandHandler : ICommandHandler<AddRoomTypeCommand, AppResult<Guid>>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICatalogUnitOfWork _catalogUnitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly ICacheService _cacheService;

    public AddRoomTypeCommandHandler(
        IPropertyRepository propertyRepository,
        ICatalogUnitOfWork catalogUnitOfWork,
        IOutboxWriter outboxWriter,
        ICacheService cacheService)
    {
        _propertyRepository = propertyRepository;
        _catalogUnitOfWork = catalogUnitOfWork;
        _outboxWriter = outboxWriter;
        _cacheService = cacheService;
    }

    public async Task<AppResult<Guid>> HandleAsync(AddRoomTypeCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);

        if (property is null)
        {
            return AppResult.Failure<Guid>(ResultError.NotFound("Property.NotFound", "Property not found."));
        }

        var moneyResult = Money.Create(command.Amount, command.Currency);

        if (moneyResult.IsFailure)
        {
            return AppResult.Failure<Guid>(moneyResult.Error);
        }

        var roomTypeResult = property.AddRoomType(
            command.Name,
            command.Capacity,
            command.TotalRooms,
            moneyResult.Value);

        if (roomTypeResult.IsFailure)
        {
            return AppResult.Failure<Guid>(roomTypeResult.Error);
        }

        var payload = PropertyCatalogChangedPayloadFactory.Create(property);

        await _outboxWriter.WriteAsync(
            CatalogIntegrationEventTypes.PropertyCatalogChanged,
            payload,
            DateTime.UtcNow,
            cancellationToken);

        await _catalogUnitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(PropertyCacheKeys.Detail(command.PropertyId), cancellationToken);

        return AppResult.Success(roomTypeResult.Value.Id);
    }
}
