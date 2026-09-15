namespace NovaCore.Auth.Application.Abstractions.Persistence.Devices;

public interface IDeviceReadService
{
    Task<Device?> GetByFingerprintAsync(Guid accountId, string fingerprint, CancellationToken ct = default);
}
