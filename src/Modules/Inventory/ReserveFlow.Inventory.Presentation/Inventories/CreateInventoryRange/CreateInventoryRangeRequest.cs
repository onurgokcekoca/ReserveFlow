namespace ReserveFlow.Inventory.Presentation.Inventories.Availability;

public sealed record CreateInventoryRangeRequest(DateOnly StartDate, DateOnly EndDate, int TotalRooms);
