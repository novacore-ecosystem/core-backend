using NovaCore.Payment.Persistence.Engine;

namespace NovaCore.Payment.Persistence.Storage.Seeders;

/// <summary>Seeds the PaymentMethod/PaymentGateway reference catalog at startup - not business data, matches Product/Auth/User/Inventory services' own seeding convention (Storage/Seeders/*Seeder.cs run from ApplicationPipeline.cs).</summary>
public sealed class PaymentSeeder(PaymentDbContext context)
{
    public async Task SeedAsync()
    {
        await SeedPaymentMethodsAsync();
        await SeedPaymentGatewaysAsync();
    }

    private async Task SeedPaymentMethodsAsync()
    {
        if (await context.PaymentMethods.AnyAsync())
            return;

        var methods = new[]
        {
            PaymentMethod.Create(MethodId(1), "VISA", "Visa", PaymentMethodType.Card),
            PaymentMethod.Create(MethodId(2), "MASTERCARD", "MasterCard", PaymentMethodType.Card),
            PaymentMethod.Create(MethodId(3), "PAYPAL", "PayPal", PaymentMethodType.DigitalWallet),
            PaymentMethod.Create(MethodId(4), "VNPAY", "VNPay", PaymentMethodType.EWallet),
            PaymentMethod.Create(MethodId(5), "MOMO", "MoMo", PaymentMethodType.EWallet),
            PaymentMethod.Create(MethodId(6), "APPLE_PAY", "Apple Pay", PaymentMethodType.DigitalWallet),
            PaymentMethod.Create(MethodId(7), "GOOGLE_PAY", "Google Pay", PaymentMethodType.DigitalWallet),
            PaymentMethod.Create(MethodId(8), "COD", "Cash on Delivery", PaymentMethodType.Cod),
        };

        context.PaymentMethods.AddRange(methods);
        await context.SaveChangesAsync();
    }

    private async Task SeedPaymentGatewaysAsync()
    {
        if (await context.PaymentGateways.AnyAsync())
            return;

        var gateways = new[]
        {
            PaymentGateway.Create(GatewayId(1), "STRIPE", "Stripe", GatewayType.Stripe),
            PaymentGateway.Create(GatewayId(2), "PAYPAL", "PayPal", GatewayType.PayPal),
            PaymentGateway.Create(GatewayId(3), "VNPAY", "VNPay", GatewayType.VNPay),
            PaymentGateway.Create(GatewayId(4), "MOMO", "MoMo", GatewayType.MoMo),
            PaymentGateway.Create(GatewayId(5), "MANUAL", "Manual", GatewayType.Manual),
        };

        context.PaymentGateways.AddRange(gateways);
        await context.SaveChangesAsync();
    }

    // Deterministic IDs so seeded reference rows keep the same Id across environments and
    // re-seeds, following ProductSeeder's own convention (see its remarks).
    private const string SeedPrefix = "019fc900-8a11-7c22";

    private static Guid MethodId(int seed) => Guid.Parse($"{SeedPrefix}-8001-{seed:x12}");

    private static Guid GatewayId(int seed) => Guid.Parse($"{SeedPrefix}-8002-{seed:x12}");
}
