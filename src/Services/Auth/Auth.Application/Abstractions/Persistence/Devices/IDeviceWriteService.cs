namespace NovaCore.Auth.Application.Abstractions.Persistence.Devices;

public interface IDeviceWriteService
{
    /// <summary>Non-committing - callers own the surrounding IUnitOfWork.ExecuteTransactionAsync/SaveChangesAsync.</summary>
    Task CreateAsync(Device device, CancellationToken ct = default);

    /// <summary>Non-committing, same reason as CreateAsync.</summary>
    Task TouchAsync(Guid deviceId, CancellationToken ct = default);
}
