using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace ReserveFlow.IntegrationTests.Infrastructure;

public sealed class ReserveFlowFixture : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("reserveflow_integration_test")
        .WithUsername("reserveflow_test")
        .WithPassword("reserveflow_test")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-management")
        .Build();

    private readonly IContainer _elasticsearch = new ContainerBuilder("docker.elastic.co/elasticsearch/elasticsearch:9.3.0")
        .WithPortBinding(9200, true)
        .WithEnvironment("discovery.type", "single-node")
        .WithEnvironment("xpack.security.enabled", "false")
        .WithEnvironment("xpack.security.http.ssl.enabled", "false")
        .WithEnvironment("xpack.ml.enabled", "false")
        .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request
                .ForPort(9200)
                .ForPath("/_cluster/health")))
        .Build();

    private WebApplicationFactory<Program>? _application;

    public HttpClient ApiClient { get; private set; } = null!;
    public HttpClient ElasticClient { get; private set; } = null!;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string ElasticsearchUrl => $"http://{_elasticsearch.Hostname}:{_elasticsearch.GetMappedPublicPort(9200)}";

    public Uri RabbitMqUri => new(_rabbitMq.GetConnectionString());

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _rabbitMq.StartAsync(),
            _elasticsearch.StartAsync());

        ElasticClient = new HttpClient { BaseAddress = new Uri(ElasticsearchUrl + "/") };

        _application = new ReserveFlowWebApplicationFactory(this);
        ApiClient = _application.CreateClient();
    }

    public void Dispose()
    {
        ApiClient?.Dispose();
        ElasticClient?.Dispose();
        _application?.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task DisposeAsync()
    {
        Dispose();

        await _elasticsearch.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
