using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReserveFlow.Search.Application.Common;
using ReserveFlow.Search.Infrastructure.Elasticsearch;
using ReserveFlow.Search.Infrastructure.Messaging;

namespace ReserveFlow.Search.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        services.AddSingleton(sp =>
        {
            ElasticsearchOptions options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;

            var settings = new ElasticsearchClientSettings(new Uri(options.Url));

            return new ElasticsearchClient(settings);
        });

        services.AddSingleton<ElasticsearchIndexInitializer>();
        services.AddSingleton<IPropertySearchRepository, ElasticsearchPropertySearchRepository>();

        services.Configure<SearchMessagingOptions>(configuration.GetSection(SearchMessagingOptions.SectionName));

        services.AddSingleton<SearchMessagingTopology>();
        services.AddHostedService<SearchCatalogConsumerWorker>();

        return services;
    }

    public static async Task InitializeSearchInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ElasticsearchIndexInitializer initializer = serviceProvider.GetRequiredService<ElasticsearchIndexInitializer>();
        SearchMessagingTopology topology = serviceProvider.GetRequiredService<SearchMessagingTopology>();

        await topology.EnsureAsync(cancellationToken);
        await initializer.InitializeAsync(cancellationToken);
    }
}
