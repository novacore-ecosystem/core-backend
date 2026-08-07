global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using NovaCore.BuildingBlock.Domain.Metadata;
global using NovaCore.BuildingBlock.Domain.ValueObjects;
global using NovaCore.BuildingBlock.Persistence.Ef.Configurations;

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
global using NovaCore.Promotion.Domain.Entities.Validations;
global using NovaCore.Promotion.Domain.Entities.Audits;
global using NovaCore.Promotion.Domain.Enums;
global using NovaCore.Promotion.Domain.Metadata;
global using NovaCore.Promotion.Domain.ValueObjects;

// "Promotion" collides with this project's own root namespace (NovaCore.Promotion.Persistence,
// NovaCore.Promotion.Domain, ...) - C# resolves the bare identifier to the namespace before the
// imported type, so the entity needs an alias (same issue Payment/Order/Product/User document for
// their own root-namespaced entity).
global using PromotionEntity = NovaCore.Promotion.Domain.Entities.Promotions.Promotion;
