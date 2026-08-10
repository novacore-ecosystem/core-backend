using NovaCore.BuildingBlock.Contract.Protos.Inventory;

using Grpc.Core;

using NovaCore.Inventory.Application.Features.Inventories.Commands.RestockStock;

using AppDeductStockItem = NovaCore.Inventory.Application.Features.Inventories.Commands.DeductStock.DeductStockItem;
using DeductStockCommand = NovaCore.Inventory.Application.Features.Inventories.Commands.DeductStock.DeductStockCommand;
using GetProductStockQuery = NovaCore.Inventory.Application.Features.Inventories.Queries.GetProductStock.GetProductStockQuery;
using GetProductsStockQuery = NovaCore.Inventory.Application.Features.Inventories.Queries.GetProductsStock.GetProductsStockQuery;

namespace NovaCore.Inventory.API.GrpcServices;

/// <summary>Thin adapter for Order Service (or any other gRPC caller) - parses the request, dispatches the same query/commands the REST endpoints use, no business logic here.</summary>
public sealed class InventoryGrpcServiceImpl(ISender sender) : InventoryGrpcService.InventoryGrpcServiceBase
{
    public override async Task<GetProductStockResponse> GetProductStock(GetProductStockRequest request, ServerCallContext context)
    {
        Guid? productVariationId = request.HasVariantId && !string.IsNullOrEmpty(request.VariantId)
            ? Guid.Parse(request.VariantId)
            : null;

        var query = new GetProductStockQuery(Guid.Parse(request.ProductId), productVariationId);
        var result = await sender.Send(query, context.CancellationToken);

        return new GetProductStockResponse
        {
            ProductId = result.ProductId.ToString(),
            VariantId = result.VariantId?.ToString() ?? string.Empty,
            TotalQuantity = result.TotalQuantity,
        };
    }

    public override async Task<GetProductsStockResponse> GetProductsStock(GetProductsStockRequest request, ServerCallContext context)
    {
        var variationIds = request.VariantIds.Select(Guid.Parse).ToArray();

        var query = new GetProductsStockQuery(variationIds);
        var result = await sender.Send(query, context.CancellationToken);

        var response = new GetProductsStockResponse();
        response.Items.AddRange(result.Select(r => new VariantStock
        {
            VariantId = r.VariantId.ToString(),
            TotalQuantity = r.TotalQuantity,
        }));

        return response;
    }

    public override async Task<DeductStockResponse> DeductStock(DeductStockRequest request, ServerCallContext context)
    {
        var items = request.Items
            .Select(i => new AppDeductStockItem(Guid.Parse(i.VariantId), i.Quantity))
            .ToList();

        var command = new DeductStockCommand(
            Guid.Parse(request.DeductionId),
            items,
            string.IsNullOrEmpty(request.Reason) ? null : request.Reason);

        var result = await sender.Send(command, context.CancellationToken);

        var response = new DeductStockResponse
        {
            Success = result.Success,
            FailureCode = result.FailureCode ?? string.Empty,
        };

        response.InsufficientItems.AddRange(result.InsufficientItems.Select(i => new NovaCore.BuildingBlock.Contract.Protos.Inventory.InsufficientStockItem
        {
            VariantId = i.VariantId.ToString(),
            RequestedQuantity = i.RequestedQuantity,
            AvailableQuantity = i.AvailableQuantity,
        }));

        return response;
    }

    public override async Task<RestockStockResponse> RestockStock(RestockStockRequest request, ServerCallContext context)
    {
        var command = new RestockStockCommand(
            Guid.Parse(request.DeductionId),
            string.IsNullOrEmpty(request.Reason) ? null : request.Reason);

        var result = await sender.Send(command, context.CancellationToken);

        return new RestockStockResponse { Success = result.Success };
    }
}
