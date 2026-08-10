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

global using NovaCore.Payment.Domain.Entities.Payments;
global using NovaCore.Payment.Domain.Enums;
global using NovaCore.Payment.Domain.ValueObjects;

// Deliberately NOT globally using NovaCore.BuildingBlock.Domain.ValueObjects here - it also
// declares a Money type, which would collide with this service's own currency-aware
// Payment.Domain.ValueObjects.Money.

// "Payment" collides with this project's own root namespace (NovaCore.Payment.Application, NovaCore.Payment.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using PaymentEntity = NovaCore.Payment.Domain.Entities.Payments.Payment;
