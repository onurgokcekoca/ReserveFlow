namespace ReserveFlow.Inventory.Presentation.Inventories.ReserveInventoryRange;

public sealed record ReserveInventoryRangeRequest(DateOnly CheckIn, DateOnly CheckOut, int Rooms);
