using ReserveFlow.Shared.Kernel.Results;

namespace ReserveFlow.Catalog.Domain.Properties
{
    public sealed record Location
    {
        public string Country { get; }
        public string AddressLine { get; }
        public string City { get; }
        public decimal? Latitude { get; }
        public decimal? Longitude { get; }

        private Location(string country, string city, string addressLine, decimal? latitude, decimal? longitude)
        {
            Country = country;
            City = city;
            AddressLine = addressLine;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static AppResult<Location> Create(string? country, string? city, string? addressLine, decimal? latitude, decimal? longitude)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.CountryRequired", "Country is required."));
            }

            if (string.IsNullOrWhiteSpace(addressLine))
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.AddressRequired", "Address is required."));
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.CityRequired", "City is required."));
            }

            if (latitude < -90 || latitude > 90)
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.InvalidLatitude", "Latitude must be between -90 and 90."));
            }

            if (longitude < -180 || longitude > 180)
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.InvalidLongitude", "Longitude must be between -180 and 180."));
            }

            if (latitude == null && longitude != null)
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.IncompleteCoordinates", "Latitude is required when longitude is provided."));
            }

            if (latitude != null && longitude == null)
            {
                return AppResult.Failure<Location>(ResultError.Validation("Location.IncompleteCoordinates", "Longitude is required when latitude is provided."));
            }

            return AppResult.Success(new Location(country.Trim(), city.Trim(), addressLine.Trim(), latitude, longitude));
        }
    }
}
