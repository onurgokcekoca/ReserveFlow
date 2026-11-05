using ReserveFlow.Catalog.Domain.Properties;

namespace ReserveFlow.Catalog.Application.Properties.SearchProperties
{
    public class PropertyListItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public PropertyType PropertyType { get; set; }
        public string Country { get; set; } = default!;
        public string City { get; set; } = default!;
        public decimal? BasePriceFrom { get; init; }
        public string? Currency { get; init; }
    }
}
