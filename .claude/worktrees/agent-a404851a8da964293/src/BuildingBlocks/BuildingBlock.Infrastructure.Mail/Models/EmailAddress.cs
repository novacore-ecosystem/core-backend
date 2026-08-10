namespace NovaCore.BuildingBlock.Infrastructure.Mail.Models;

public sealed record EmailAddress(
    string Address,
    string? Name = null);
