global using System;
global using System.Collections.Generic;
global using System.Linq;

global using NovaCore.BuildingBlock.Domain.Abstractions;
global using NovaCore.BuildingBlock.Domain.Exceptions;
global using NovaCore.BuildingBlock.SharedKernel.Extensions;

global using NovaCore.Payment.Domain.Enums;
global using NovaCore.Payment.Domain.ValueObjects;

// Deliberately NOT globally using NovaCore.BuildingBlock.Domain.ValueObjects here - it also
// declares a Money type, which would collide with this service's own currency-aware
// Payment.Domain.ValueObjects.Money. Reference shared VOs (Email, PhoneNumber, ...) with an
// explicit file-scoped `using` where actually needed.
