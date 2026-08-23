global using Xunit;

global using NSubstitute;

global using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
global using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
global using NovaCore.BuildingBlock.Application.Exceptions;
global using NovaCore.BuildingBlock.Domain.ValueObjects;

global using NovaCore.Content.Application.Abstractions.Persistence.Contents;
global using NovaCore.Content.Domain.Entities.Contents;
global using NovaCore.Content.Domain.Enums;
global using NovaCore.Content.Domain.Metadata;
global using NovaCore.Content.Domain.ValueObjects;

global using Shouldly;

global using ContentEntity = NovaCore.Content.Domain.Entities.Contents.Content;
