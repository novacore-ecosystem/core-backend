global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;

global using NovaCore.Order.Domain.Entities.Catalogs;
global using NovaCore.Order.Domain.Entities.Orders;
global using NovaCore.Order.Domain.Entities.Returns;
global using NovaCore.Order.Domain.Entities.Tags;

// "Order" collides with this project's own root namespace (NovaCore.Order.Persistence, NovaCore.Order.Domain, ...) -
// C# resolves the bare identifier to the namespace before the imported type, so the entity needs an alias.
global using OrderEntity = NovaCore.Order.Domain.Entities.Orders.Order;
