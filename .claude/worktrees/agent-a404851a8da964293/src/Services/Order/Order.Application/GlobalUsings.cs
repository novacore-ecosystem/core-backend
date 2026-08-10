global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
global using NovaCore.BuildingBlock.Application.Abstractions.Events;
global using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
global using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
global using NovaCore.BuildingBlock.Application.Abstractions.Services;
global using NovaCore.BuildingBlock.Application.Exceptions;
global using NovaCore.BuildingBlock.Domain.Enums;
global using NovaCore.BuildingBlock.Domain.Exceptions;
global using NovaCore.BuildingBlock.Domain.ValueObjects;
global using NovaCore.BuildingBlock.SharedKernel.Constants;

global using Mapster;

global using NovaCore.Order.Domain.Entities.Catalogs;
global using NovaCore.Order.Domain.Entities.Orders;
global using NovaCore.Order.Domain.Enums;
global using NovaCore.Order.Domain.ValueObjects;
global using OrderEntity = NovaCore.Order.Domain.Entities.Orders.Order;