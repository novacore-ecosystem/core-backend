using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Features.Warehouses.DTOs;

namespace NovaCore.Inventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed class CreateWarehouseHandler(
    IWarehouseReadService warehouseReadService,
    IWarehouseWriteService warehouseWriteService) : ICommandHandler<CreateWarehouseCommand, CreateWarehouseResponse>
{
    public async Task<CreateWarehouseResponse> Handle(CreateWarehouseCommand request, CancellationToken ct = default)
    {
        var existing = await warehouseReadService.GetByCodeAsync(request.Code, ct);
        if (existing is not null)
            throw new ConflictException($"Warehouse with code ({request.Code}) already exists");

        var warehouseId = await warehouseWriteService.CreateAsync(
            new CreateWarehouseRequest(
                request.Code.Trim(),
                request.Name.Trim(),
                request.Type,
                request.Country.Trim(),
                request.StateOrProvince.Trim(),
                request.City.Trim(),
                request.District.Trim(),
                request.Ward.Trim(),
                request.Street.Trim(),
                request.PostalCode.Trim()),
            ct);

        return new CreateWarehouseResponse(warehouseId);
    }
}
