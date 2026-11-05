using Xunit;

namespace ReserveFlow.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ReserveFlowTestGroup : ICollectionFixture<ReserveFlowFixture>
{
    public const string Name = "ReserveFlow integration containers";
}
