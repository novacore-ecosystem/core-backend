global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

global using NovaCore.BuildingBlock.Application.Abstractions.CQRS;
global using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

global using NovaCore.Product.Domain.Entities.Categories;
global using NovaCore.Product.Domain.Entities.Products;
global using NovaCore.Product.Domain.Entities.Tags;
global using NovaCore.Product.Domain.Enums;
global using NovaCore.Product.Domain.ValueObjects;
global using ProductEntity = NovaCore.Product.Domain.Entities.Products.Product;