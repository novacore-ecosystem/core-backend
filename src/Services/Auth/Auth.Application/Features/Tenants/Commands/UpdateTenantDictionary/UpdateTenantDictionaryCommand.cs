using System.Text.Json;

namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpdateTenantDictionary;

/// <summary>Bulk merge-update of one language's dictionary - the payload is merged onto the
/// stored DictionaryJson (unspecified keys preserved, see JsonMergeHelper), never a wholesale
/// replace. Other languages' dictionaries are untouched (each is a separate TenantLocale row).</summary>
public sealed record UpdateTenantDictionaryCommand(
    Guid TenantId,
    string Language,
    JsonElement Dictionary) : ICommand;
