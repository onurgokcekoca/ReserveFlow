using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ReserveFlow.IntegrationTests.Infrastructure;
using Xunit;

namespace ReserveFlow.IntegrationTests.CatalogSearch;

[Collection(ReserveFlowTestGroup.Name)]
public sealed class CatalogSearchFlowTests(ReserveFlowFixture fixture)
{
    [Fact]
    public async Task PropertyAndRoomsAreEventuallyProjectedIntoElasticsearch()
    {
        string uniqueName = "Integration Hotel " + Guid.NewGuid().ToString("N")[..8];

        using HttpResponseMessage create = await fixture.ApiClient.PostAsJsonAsync(
            "/properties",
            new
            {
                name = uniqueName,
                description = "Testcontainers end-to-end test",
                country = "Türkiye",
                city = "Ankara",
                addressLine = "Çankaya",
                latitude = 40.9909,
                longitude = 29.0267,
                propertyType = 1
            });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using JsonDocument created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());

        Guid propertyId = Guid.Parse(created.RootElement.GetProperty("id").GetString()!);
        Assert.NotEqual(Guid.Empty, propertyId);

        await AddRoomAsync(propertyId, "Standard Room", capacity: 3);
        await AddRoomAsync(propertyId, "Family Room", capacity: 5);

        JsonElement source = await WaitForLatestProjectionAsync(propertyId);

        Assert.Equal(uniqueName, source.GetProperty("name").GetString());
        Assert.Equal("Ankara", source.GetProperty("city").GetString());
        Assert.Equal(5, source.GetProperty("maxGuests").GetInt32());

        string?[] roomNames = source.GetProperty("roomTypeNames")
            .EnumerateArray()
            .Select(x => x.GetString())
            .ToArray();

        Assert.Contains("Standard Room", roomNames);
        Assert.Contains("Family Room", roomNames);
        Assert.Equal(2, roomNames.Length);
    }

    private async Task AddRoomAsync(Guid propertyId, string name, int capacity)
    {
        using HttpResponseMessage response = await fixture.ApiClient.PostAsJsonAsync(
            $"/properties/{propertyId}/room-types",
            new
            {
                name,
                capacity,
                totalRooms = 10,
                basePriceAmount = 2500,
                currency = "TRY"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<JsonElement> WaitForLatestProjectionAsync(Guid propertyId)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(30);
        Stopwatch clock = Stopwatch.StartNew();
        string? lastBody = null;

        while (clock.Elapsed < timeout)
        {
            using HttpResponseMessage response = await fixture.ElasticClient.GetAsync($"reserveflow-properties/_doc/{propertyId:D}");

            lastBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using JsonDocument json = JsonDocument.Parse(lastBody);
                JsonElement source = json.RootElement.GetProperty("_source");

                if (source.TryGetProperty("maxGuests", out JsonElement guests) &&
                    guests.GetInt32() == 5 &&
                    source.TryGetProperty("roomTypeNames", out JsonElement rooms) &&
                    rooms.EnumerateArray().Any(x => x.GetString() == "Family Room"))
                {
                    return source.Clone();
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new TimeoutException(
            $"Property {propertyId} did not reach its latest ES snapshot in {timeout}. " +
            $"Last Elasticsearch response: {lastBody}");
    }
}
