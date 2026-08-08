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
global using NovaCore.BuildingBlock.SharedKernel.Constants;

global using Mapster;

global using NovaCore.Promotion.Domain.Entities.Campaigns;
global using NovaCore.Promotion.Domain.Entities.Promotions;
global using NovaCore.Promotion.Domain.Entities.Coupons;
global using NovaCore.Promotion.Domain.Entities.Vouchers;
global using NovaCore.Promotion.Domain.Entities.Loyalty;
global using NovaCore.Promotion.Domain.Entities.Rewards;
global using NovaCore.Promotion.Domain.Entities.Distributions;
global using NovaCore.Promotion.Domain.Entities.Recommendations;
global using NovaCore.Promotion.Domain.Entities.ProductSets;
global using NovaCore.Promotion.Domain.Entities.Gifts;
global using NovaCore.Promotion.Domain.Entities.Approvals;
global using NovaCore.Promotion.Domain.Enums;

// "Promotion" collides with this project's own root namespace (NovaCore.Promotion.Application,
// NovaCore.Promotion.Domain, ...) - same alias Promotion.Persistence/GlobalUsings.cs documents.
global using PromotionEntity = NovaCore.Promotion.Domain.Entities.Promotions.Promotion;
