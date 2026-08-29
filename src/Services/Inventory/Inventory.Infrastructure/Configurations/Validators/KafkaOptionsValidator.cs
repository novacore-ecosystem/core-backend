using NovaCore.BuildingBlock.Messaging.Kafka.Configuration;

using FluentValidation;

namespace NovaCore.Inventory.Infrastructure.Configurations.Validators;

/// <summary>
/// Local copy of the fail-fast rules for <see cref="KafkaOptions"/>. It lives in BuildingBlock.Messaging.Kafka
/// and isn't an ISetting - it's bound by AddKafkaMessaging, outside this assembly's scan - so
/// ConfigurationExtensions runs this validator manually against the same section.
/// </summary>
public sealed class KafkaOptionsValidator : AbstractValidator<KafkaOptions>
{
    public KafkaOptionsValidator()
    {
        RuleFor(x => x.BootstrapServers).NotEmpty().WithMessage("BootstrapServers is required.");
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("GroupId is required.");
        RuleFor(x => x.ConsumerWorkersCount).GreaterThan(0).WithMessage("ConsumerWorkersCount must be positive.");
        RuleFor(x => x.ConsumerBufferSize).GreaterThan(0).WithMessage("ConsumerBufferSize must be positive.");

        When(x => x.EnableSecurityProtocol, () =>
        {
            RuleFor(x => x.SaslMechanism).NotEmpty().WithMessage("SaslMechanism is required when EnableSecurityProtocol is true.");
            RuleFor(x => x.SaslUsername).NotEmpty().WithMessage("SaslUsername is required when EnableSecurityProtocol is true.");
            RuleFor(x => x.SaslPassword).NotEmpty().WithMessage("SaslPassword is required when EnableSecurityProtocol is true.");
        });
    }
}
