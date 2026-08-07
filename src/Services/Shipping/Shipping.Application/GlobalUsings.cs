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

global using NovaCore.Shipping.Domain.Entities.CarrierIntegrations;
global using NovaCore.Shipping.Domain.Entities.Deliveries;
global using NovaCore.Shipping.Domain.Entities.Pickups;
global using NovaCore.Shipping.Domain.Entities.Providers;
global using NovaCore.Shipping.Domain.Entities.ReturnShipments;
global using NovaCore.Shipping.Domain.Entities.Shipments;
global using NovaCore.Shipping.Domain.Entities.ShippingProfiles;
global using NovaCore.Shipping.Domain.Entities.TransportationPeople;
global using NovaCore.Shipping.Domain.Entities.Transportations;
global using NovaCore.Shipping.Domain.Entities.TransportationVehicles;
global using NovaCore.Shipping.Domain.Entities.VerifiedAddresses;
global using NovaCore.Shipping.Domain.Enums;
global using NovaCore.Shipping.Domain.ValueObjects;
