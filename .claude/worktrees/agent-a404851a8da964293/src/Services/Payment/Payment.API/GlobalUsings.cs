global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Carter;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

global using NovaCore.Payment.Domain.Enums;
global using NovaCore.Payment.Domain.ValueObjects;

// Deliberately NOT globally using NovaCore.BuildingBlock.Domain.ValueObjects here - it also
// declares a Money type, which would collide with this service's own currency-aware
// Payment.Domain.ValueObjects.Money.
