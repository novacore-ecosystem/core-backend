global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

global using NovaCore.BuildingBlock.Application.Abstractions.Common;
global using NovaCore.BuildingBlock.Criteria.Requests;
global using NovaCore.BuildingBlock.Persistence.Ef.Configurations;

global using NovaCore.Inventory.Domain.Entities;
global using NovaCore.Inventory.Domain.Entities.Inventories;
global using NovaCore.Inventory.Domain.Entities.InventoryTransactions;
global using NovaCore.Inventory.Domain.Entities.InventoryLots;
global using NovaCore.Inventory.Domain.Entities.InventoryReservations;
global using NovaCore.Inventory.Domain.Entities.InventorySerials;
global using NovaCore.Inventory.Domain.Entities.InventoryCounts;
global using NovaCore.Inventory.Domain.Entities.InventoryDocuments;
global using NovaCore.Inventory.Domain.Entities.Warehouses;
global using NovaCore.Inventory.Domain.Enums;

global using NovaCore.BuildingBlock.Domain.ValueObjects;
global using NovaCore.BuildingBlock.Persistence.Ef.DbContext;
global using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
global using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
