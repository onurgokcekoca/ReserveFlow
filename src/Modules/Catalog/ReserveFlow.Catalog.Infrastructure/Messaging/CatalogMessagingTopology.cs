namespace ReserveFlow.Catalog.Infrastructure.Messaging
{
    internal static class CatalogMessagingTopology
    {
        public const string ExchangeName = "catalog.events";

        public const string PropertyCatalogChangedRoutingKey = "property.catalog.changed";
    }
}
