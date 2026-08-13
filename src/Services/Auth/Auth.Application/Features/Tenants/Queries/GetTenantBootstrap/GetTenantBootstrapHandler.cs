using NovaCore.Auth.Application.Abstractions.Persistence.TenantClients;
using NovaCore.Auth.Application.Abstractions.Persistence.Tenants;
using NovaCore.Auth.Application.Common;

namespace NovaCore.Auth.Application.Features.Tenants.Queries.GetTenantBootstrap;

public sealed class GetTenantBootstrapHandler(
    ITenantClientReadService tenantClientReadService,
    ITenantReadService tenantReadService) : IQueryHandler<GetTenantBootstrapQuery, TenantBootstrapResponse>
{
    public async Task<TenantBootstrapResponse> Handle(GetTenantBootstrapQuery request, CancellationToken ct = default)
    {
        // Same generic-failure shape as LoginHandler - an invalid/unknown/revoked client key and
        // "no tenant" both surface identically, so a caller can't enumerate valid client keys.
        var tenantClient = await tenantClientReadService.GetByPublicKeyAsync(request.ClientPublicKey, ct);
        if (tenantClient is null || !tenantClient.IsUsable())
            throw new UnauthorizedException("Invalid client key.");

        // TODO: the Root client has no Tenant to bootstrap - confirm whether the Root Portal needs
        // its own bootstrap contract (nova-console) or genuinely never calls this endpoint.
        if (tenantClient.TenantId is not { } tenantId)
            throw new BadRequestException("The Root client has no tenant bootstrap.");

        var tenant = await tenantReadService.GetByIdAsync(tenantId, ct)
            ?? throw new NotFoundException("Tenant", tenantId);

        if (!tenant.IsActive)
            throw new ConflictException("Tenant is disabled.");

        var translations = TenantTranslationMerger.BuildEffective(tenant, LanguageCodeConstant.SupportedLanguages);

        return new TenantBootstrapResponse(
            tenant.Version,
            new TenantBootstrapInfo(tenant.Id, tenant.Code.Value, tenant.Name, tenant.LogoUrl, tenant.FaviconUrl),
            LanguageCodeConstant.SupportedLanguages,
            translations);
    }
}
