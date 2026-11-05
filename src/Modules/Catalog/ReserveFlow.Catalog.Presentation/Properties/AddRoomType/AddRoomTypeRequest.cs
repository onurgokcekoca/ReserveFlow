namespace ReserveFlow.Catalog.Presentation.Properties.AddRoomType;

public sealed record AddRoomTypeRequest(
    string? Name,
    int Capacity,
    int TotalRooms,
    decimal BasePriceAmount,
    string? Currency);
