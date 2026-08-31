using Microsoft.EntityFrameworkCore;
using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;
using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Write;

public sealed class TenantWriteService(
    ITenantRepository repo,
    ITenantClientWriteService tenantClientWrite,
    IUnitOfWork unitOfWork) : ITenantWriteService, IPersistenceService
{
    public async Task CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        await repo.AddAsync(tenant, ct);
        await tenantClientWrite.CreateAsync(tenant.Id, tenant.Name, ct);
    }

    public async Task UpdateAsync(Guid id, Action<Tenant> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(t => t.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateWithLocalesAsync(Guid id, Action<Tenant> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(t => t.Id == id, q => q.Include(t => t.Locales), update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<Tenant> UpsertLocaleAsync(
        Guid id,
        LanguageCode? language,
        string? configurationJson = null,
        string? dictionaryJson = null,
        CancellationToken ct = default)
    {
        if (configurationJson.IsNullOrWhiteSpace() && dictionaryJson.IsNotNullOrWhiteSpace())
            throw new BadRequestException("At least one of the two configurations must exist");

        Tenant updatedTenant = null!;
        await repo.UpdateAsync(
            predicate: t => t.Id == id,
            includes: query => query.Include(t => t.Locales),
            updateAction: tenant =>
            {
                var existing = tenant.Locales
                    .FirstOrDefault(l => l.LanguageCode == language);

                // Merge new config with current config if existing
                var configuration = existing?.ConfigurationJson ?? "{}";
                if (configurationJson.IsNotNullOrWhiteSpace())
                {
                    var mergedConfig = JsonMergeHelper.Merge(configuration, configurationJson);
                    configuration = mergedConfig.ToJsonString();
                }

                // Merge new dictionary with current dictionary if existing
                var dictionary = existing?.DictionaryJson ?? "{}";
                if (dictionaryJson.IsNotNullOrWhiteSpace())
                {
                    var mergedDictionary = JsonMergeHelper.Merge(dictionary, dictionaryJson);
                    dictionary = mergedDictionary.ToJsonString();
                }

                // Upsert tenant locale
                tenant.SetLocale(language, configuration, dictionary);
                tenant.IncrementVersion();
                updatedTenant = tenant;
            },
            ct);
        return updatedTenant;
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            t => t.Id == id,
            t => t.Deactivate(),
            ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.UpdateAsync(
            t => t.Id == id,
            t => t.Delete(),
            ct);
    }
}
