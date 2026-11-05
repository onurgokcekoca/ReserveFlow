using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using ReserveFlow.Search.Application.Common.Models;

namespace ReserveFlow.Search.Infrastructure.Elasticsearch;

internal sealed class ElasticsearchIndexInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchIndexInitializer(ElasticsearchClient client, IOptions<ElasticsearchOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var existsResponse = await _client.Indices.ExistsAsync(_options.IndexName, cancellationToken);

        if (existsResponse.Exists)
        {
            return;
        }

        var createResponse = await _client.Indices.CreateAsync<PropertySearchDocument>(
                index => index
                    .Index(_options.IndexName)
                    .Mappings(mappings => mappings
                        .Properties(properties => properties
                            .Keyword(x => x.PropertyId)
                            .Text(x => x.Name)
                            .Text(x => x.Description)
                            .Keyword(x => x.Country)
                            .Keyword(x => x.City)
                            .Text(x => x.AddressLine)
                            .Keyword(x => x.PropertyType)
                            .Keyword(x => x.ProviderId)
                            .IntegerNumber(x => x.MaxGuests)
                            .Keyword(x => x.RoomTypeIds)
                            .Text(x => x.RoomTypeNames)
                        )),
                cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create Elasticsearch index '{_options.IndexName}'. " + createResponse.DebugInformation);
        }
    }
}
