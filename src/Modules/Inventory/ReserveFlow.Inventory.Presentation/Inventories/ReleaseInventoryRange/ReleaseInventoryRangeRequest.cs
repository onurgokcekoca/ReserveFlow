namespace ReserveFlow.Inventory.Presentation.Inventories.ReleaseInventoryRange;

public sealed record ReleaseInventoryRangeRequest(DateOnly checkIn, DateOnly checkOut, int rooms);
