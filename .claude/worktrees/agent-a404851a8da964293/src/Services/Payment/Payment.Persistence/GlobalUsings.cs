global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using NovaCore.BuildingBlock.Persistence.Ef.Configurations;

global using NovaCore.Payment.Domain.Entities.Payments;
global using NovaCore.Payment.Domain.Entities.Catalogs;
global using NovaCore.Payment.Domain.Entities.Accounts;
global using NovaCore.Payment.Domain.Entities.Billing;
global using NovaCore.Payment.Domain.Entities.Operations;
global using NovaCore.Payment.Domain.Entities.Webhooks;
global using NovaCore.Payment.Domain.Entities.Scheduling;
global using NovaCore.Payment.Domain.Enums;
global using NovaCore.Payment.Domain.ValueObjects;

// "Payment" collides with this project's own root namespace (NovaCore.Payment.Persistence, NovaCore.Payment.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using PaymentEntity = NovaCore.Payment.Domain.Entities.Payments.Payment;
