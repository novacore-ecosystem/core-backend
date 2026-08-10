namespace NovaCore.Inventory.Application.Features.Inventories.DTOs;

public sealed record InventoryAdjustmentResult(InventoryStock Entity, int Delta);
