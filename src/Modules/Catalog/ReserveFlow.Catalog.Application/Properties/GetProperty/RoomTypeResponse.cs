namespace ReserveFlow.Catalog.Application.Properties.GetProperty;

public class RoomTypeResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public int Capacity { get; init; }
    public int TotalRooms { get; init; }
    public decimal BasePriceAmount { get; init; }
    public string? Currency { get; init; }
}
