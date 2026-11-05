using ReserveFlow.Catalog.Application.Abstractions;
using ReserveFlow.Catalog.Application.Properties.IntegrationEvents;
using ReserveFlow.Catalog.Domain.Properties;
using ReserveFlow.Shared.Application.Authentication;
using ReserveFlow.Shared.Application.Messaging;
using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Application.Properties.CreateProperty
{
    internal sealed class CreatePropertyCommandHandler : ICommandHandler<CreatePropertyCommand, AppResult<Guid>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICatalogUnitOfWork _catalogUnitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IOutboxWriter _outboxWriter;

        public CreatePropertyCommandHandler(IPropertyRepository propertyRepository, ICatalogUnitOfWork catalogUnitOfWork, ICurrentUser currentUser, IOutboxWriter outboxWriter)
        {
            _propertyRepository = propertyRepository;
            _catalogUnitOfWork = catalogUnitOfWork;
            _currentUser = currentUser;
            _outboxWriter = outboxWriter;
        }

        public async Task<AppResult<Guid>> HandleAsync(CreatePropertyCommand command, CancellationToken cancellationToken)
        {
            var locationResult = Location.Create(command.Country, command.City, command.AddressLine, command.Latitude, command.Longitude);

            if (locationResult.IsFailure)
            {
                return AppResult.Failure<Guid>(locationResult.Error);
            }

            var propertyResult = Property.Create(command.Name!, command.Description!, locationResult.Value, command.PropertyType, _currentUser.UserId);

            if (propertyResult.IsFailure)
            {
                return AppResult.Failure<Guid>(propertyResult.Error);
            }

            var propertyResultValue = propertyResult.Value;

            await _propertyRepository.AddAsync(propertyResultValue, cancellationToken);

            var payload = PropertyCatalogChangedPayloadFactory.Create(propertyResultValue);

            await _outboxWriter.WriteAsync(CatalogIntegrationEventTypes.PropertyCatalogChanged, payload, DateTime.UtcNow, cancellationToken);

            await _catalogUnitOfWork.SaveChangesAsync(cancellationToken);

            return AppResult.Success(propertyResult.Value.Id);
        }
    }
}
