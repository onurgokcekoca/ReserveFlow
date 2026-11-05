using System.Globalization;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ReserveFlow.Catalog.Infrastructure.Persistence;

namespace ReserveFlow.IntegrationTests.Infrastructure;

public sealed class ReserveFlowWebApplicationFactory(ReserveFlowFixture fixture) : WebApplicationFactory<Program>
{
    private const string CatalogWorkerName = "ReserveFlow.Catalog.Infrastructure.Outbox.OutboxPublisherWorker";

    private const string SearchWorkerName = "ReserveFlow.Search.Infrastructure.Messaging.SearchCatalogConsumerWorker";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            Uri rabbit = fixture.RabbitMqUri;
            string[] userInfo = rabbit.UserInfo.Split(':', 2);

            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMq:HostName"] = rabbit.Host,
                ["RabbitMq:Port"] = rabbit.Port.ToString(CultureInfo.InvariantCulture),
                ["RabbitMq:UserName"] = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "guest",
                ["RabbitMq:Password"] = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "guest",
                ["SearchMessaging:MaxRetryCount"] = "3",
                ["SearchMessaging:RetryDelayMilliseconds"] = "15000"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.ConfigureDbContext<CatalogDbContext>(options => options.UseNpgsql(fixture.PostgresConnectionString, postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

            services.RemoveAll<ElasticsearchClient>();
            services.AddSingleton(new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(fixture.ElasticsearchUrl))));

            ServiceDescriptor[] wantedWorkers = services
                .Where(x => x.ServiceType == typeof(IHostedService) &&
                            (x.ImplementationType?.FullName == CatalogWorkerName ||
                             x.ImplementationType?.FullName == SearchWorkerName))
                .ToArray();

            if (wantedWorkers.Length != 2)
            {
                throw new InvalidOperationException(
                    $"Expected Catalog + Search workers, found {wantedWorkers.Length}. " +
                    "Check worker class names/DI registrations.");
            }

            ServiceDescriptor[] unrelatedWorkers = services
                .Where(x => x.ServiceType == typeof(IHostedService) &&
                            x.ImplementationType?.FullName?.StartsWith("ReserveFlow.", StringComparison.Ordinal) == true &&
                            x.ImplementationType.FullName != CatalogWorkerName &&
                            x.ImplementationType.FullName != SearchWorkerName)
                .ToArray();

            foreach (ServiceDescriptor worker in unrelatedWorkers)
            {
                services.Remove(worker);
            }

            int firstWorkerIndex = -1;
            for (int i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType == typeof(IHostedService) &&
                    (services[i].ImplementationType?.FullName == CatalogWorkerName ||
                     services[i].ImplementationType?.FullName == SearchWorkerName))
                {
                    firstWorkerIndex = i;
                    break;
                }
            }

            services.Insert(firstWorkerIndex, ServiceDescriptor.Singleton<IHostedService, CatalogMigrationHostedService>());
        });
    }
}
