namespace NovaCore.Auth.Application.Features.Tenants.Commands.UpsertTenantTranslation;

/// <summary>Key-level upsert into one language's DictionaryJson - merges a single key/value onto
/// the stored dictionary, preserving every other key (see JsonMergeHelper). For a bulk
/// replace-with-merge of an entire language's dictionary, see UpdateTenantDictionaryCommand.</summary>
public sealed record UpsertTenantTranslationCommand(
    Guid TenantId,
    string Language,
    string Key,
    string Value) : ICommand;
