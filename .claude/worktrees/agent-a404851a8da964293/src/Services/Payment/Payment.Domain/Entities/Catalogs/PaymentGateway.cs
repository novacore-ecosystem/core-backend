namespace NovaCore.Payment.Domain.Entities.Catalogs;

/// <summary>Catalog of supported payment gateways (Stripe, PayPal, VNPay, MoMo, Adyen, ...). Reference/lookup data seeded via migration.</summary>
public sealed class PaymentGateway : AggregateRoot<Guid>, IAuditable
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public GatewayType GatewayType { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<GatewayConfiguration> Configurations { get; private set; } = [];
    public ICollection<GatewayStatusMapping> StatusMappings { get; private set; } = [];

    private PaymentGateway() { }

    public static PaymentGateway Create(Guid id, string code, string name, GatewayType gatewayType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw ExceptionFactory.RequiredField("Gateway code cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Gateway name cannot be empty.");

        return new PaymentGateway
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            GatewayType = gatewayType,
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    /// <summary>Only PaymentGateway may construct a GatewayConfiguration (see GatewayConfiguration.Create being internal).</summary>
    public GatewayConfiguration AddConfiguration(GatewayEnvironment environment, string apiKeyRef, string secretRef, string? webhookSecretRef = null)
    {
        var configuration = GatewayConfiguration.Create(Id, environment, apiKeyRef, secretRef, webhookSecretRef);
        Configurations.Add(configuration);
        return configuration;
    }

    /// <summary>Only PaymentGateway may construct a GatewayStatusMapping (see GatewayStatusMapping.Create being internal).</summary>
    public GatewayStatusMapping AddStatusMapping(string gatewayStatusCode, PaymentStatus mappedStatus, string? description = null)
    {
        var mapping = GatewayStatusMapping.Create(Id, gatewayStatusCode, mappedStatus, description);
        StatusMappings.Add(mapping);
        return mapping;
    }
}
