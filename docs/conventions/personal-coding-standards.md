# Personal Coding Standards

**Scope:** reusable C#/.NET style rules that are **not** specific to NovaCore's architecture — they apply to any C# backend project. Unlike every other doc under `conventions/`, this one is written to be portable: copy it to another project's own conventions doc and it still holds. Where a rule below only makes sense inside NovaCore (transaction boundaries, `DbContext` access, repository shape), it lives in [persistence-coding-conventions.md](persistence-coding-conventions.md) instead — see the "NovaCore-specific" pointer at the bottom of this doc.

This doc is one of the two Coding Convention docs [04-coding-rules.md](../04-coding-rules.md) is split from: that doc owns NovaCore-specific naming/CQRS/DI shape, this one owns general C# style. Formatting rules that were already established in `04-coding-rules.md` (parameter wrapping, method chains, ternaries) are **not** duplicated here — see [04-coding-rules.md#formatting](../04-coding-rules.md#formatting) for those; this doc only adds what wasn't covered there.

## Language / syntax currency

Target: **.NET 10 / C# 13** (see `Directory.Build.props`). Use the current syntax the target version actually supports instead of an older equivalent that does the same thing — collection expressions, primary constructors, raw string literals, `required` members, pattern matching over manual type-checks, file-scoped namespaces, target-typed `new()`. Before reaching for older syntax "to be safe," check the project's actual `TargetFramework`/`LangVersion` rather than assuming — don't write code that looks older than the project it's in, and don't introduce a compatibility pattern (a manual null-check where a null-coalescing/pattern-matching form already exists, a `switch` statement where a `switch` expression reads more clearly) that only existed for an older language version.

## External data and property naming

When consuming data from an external API, MongoDB, a queue payload, or any system whose serialized property names use `lowerCamelCase` (or another convention foreign to this codebase), the internal C# property still follows normal PascalCase C# naming — never copy the external casing/name onto the C# property to save a mapping attribute. Map explicitly at the serialization boundary instead:

```csharp
[JsonPropertyName("externalPropertyName")]
public string ExternalPropertyName { get; set; }
```

(Substitute the attribute the project's serializer actually uses — `System.Text.Json`'s `[JsonPropertyName]`, MongoDB.Driver's `[BsonElement]`, etc. For this codebase specifically, see [reference/serialization.md](../reference/serialization.md) for which `JsonSerializerOptions` instance to serialize through.)

The external contract belongs to the serialization boundary; the internal model does not inherit a third party's naming convention. Normalize to a clear, conventional C# name and let the mapping attribute carry the translation.

## Warning-free code

New or modified code should already be warning-clean against the project's actual analyzer configuration when it's written, not left for review to catch. Concretely:

- Resolve compiler/analyzer/IDE warnings on lines you touch — don't leave a new warning "because it still compiles."
- Don't suppress a warning (`#pragma warning disable`, `[SuppressMessage]`) without a genuine reason; when a suppression is truly needed, add a short comment saying why it's safe.
- This is a hygiene rule, not a mandate to fix unrelated pre-existing warnings in a file you're touching for something else.

## Comments and XML documentation

Comments exist so a reader understands intent **before** reading the implementation — they don't replace reading the code, and they are not a documentation-coverage target. Don't add a comment or XML summary to something whose name already says what it does.

### Properties

Normal properties use a one-line inline summary:

```csharp
/// <summary>Current stock quantity.</summary>
public int Quantity { get; private set; }
```

A property with value-dependent behavior (an enum-like string, a field whose valid values carry special meaning) uses the multiline form instead — never mixed with the inline form:

```csharp
/// <summary>
/// Property description.
/// Values:
/// 1. abc: Short value description.
/// 2. xyz: Short value description.
/// </summary>
public string Type { get; set; }
```

If a property's doc needs more than one line, the whole block goes multiline (opening tag / content / closing tag each on their own line) — don't keep the opening `<summary>` inline while wrapping only the rest.

### Methods and classes

Always use the multiline structure, even for a method with a single parameter — this keeps future parameter additions a pure line-insert instead of a reformat, which is what keeps the diff small when the signature grows:

```csharp
/// <summary>
/// Ensures the actor outranks the target account.
/// </summary>
/// <param name="actor">The acting account's snapshot.</param>
/// <param name="target">The account being managed.</param>
/// <returns>The validation result.</returns>
```

Never the inline single-line form (`/// <summary>Ensures the actor outranks the target account.</summary>`) for a method or class, and never wrap a single tag's own content across multiple lines when it fits on one (`<summary>\nEnsures the actor\noutranks the target account.\n</summary>` is wrong — keep each tag's content on one line).

**Summary** is the title, not the explanation — one or two sentences. Expand abbreviated method names into readable language, disambiguate which ID/entity is meant when more than one concept could apply (`GetUserById` → "Retrieves a user by the user's ID", not a restatement of the method name).

**Parameters** — one line each unless the parameter has multiple valid meanings or value-dependent behavior, in which case use the same numbered multiline form as the property example above. Keep descriptions readable, not abbreviated.

**Returns** — document it whenever method documentation exists and the method isn't `void`; multiline only when the return value needs the same kind of value-dependent explanation.

**`CancellationToken`** is never documented with its own `<param>` — it's standard infrastructure — but its presence as the only "real" parameter is never a reason to skip method documentation entirely:

```csharp
/// <summary>
/// Loads the user's permission snapshot.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>The user's permission snapshot.</returns>
public async Task<PermissionSnapshot> GetAsync(
    Guid userId,
    CancellationToken cancellationToken)
```

**`<remarks>`** is optional — use it only for behavior that genuinely doesn't fit in the summary: non-obvious side effects, invocation order constraints, important assumptions. Keep it as short as still explains the behavior; it is not a place to document the implementation.

### What actually needs documentation

Required: business-domain classes/methods, custom logic, abbreviated or ambiguous method names, methods carrying an important business rule, a custom persistence method whose *reason for existing* isn't obvious from its name.

Not required: conventional CQRS handler methods, conventional repository methods (`GetById`, `GetList`, `Search`), obvious CRUD, self-explanatory simple properties. For a custom repository/service method that exists because the generic/base version can't provide the behavior, document *why the custom method exists*, not what its name already communicates.

## Member ordering

For a domain/entity class, order members:

1. Local properties/constants/readonly fields
2. Entity properties
3. Entity navigation properties (collection navigations after scalar properties; a direct 1:1 navigation placed immediately below the ID/property it corresponds to, when that makes the relationship clearer)
4. Static creation methods
5. Constructor
6. Public/protected methods
7. Private methods/utilities
8. Runtime/lifecycle state (not domain data) — last

For a non-entity class, runtime/lifecycle properties stay toward the end rather than mixed in with the class's actual state.

## Regions

Use `#region` to group a large class into meaningful sections, not mechanically on every class. A region with real semantic weight gets a short separator explaining the group:

```csharp
// =================
// Permission State
// Stores the effective permission state used during authorization.
// =================
#region Permission State
...
#endregion
```

Regions exist for navigation, not to hide a class that should have been split up. (This codebase's Application handlers already use a narrower, fixed version of this pattern — region-per-responsibility, e.g. Validation/Business/Events/Cache — see [application-coding-conventions.md#responsibility-based-extraction](application-coding-conventions.md); this general rule is for everything else.)

## Boolean naming

Boolean properties and methods read as a question: prefix with `Is`/`Can`/`Has`/`Have` (`IsActive`, `CanManage`, `HasPermission`, `HasChildren`). Avoid a bare noun/verb name that doesn't visibly signal it's a boolean.

## Anti-hardcoding

A meaningful string/value checked more than once should have exactly one authoritative declaration:

1. Reuse an existing project constant if one already covers it.
2. Otherwise add one in the appropriate constant location for its scope (see [application-coding-conventions.md#constants](application-coding-conventions.md) for where Application-layer constants live in this codebase).
3. A value genuinely local to one method may stay a local constant if it doesn't belong to the broader domain.

This avoids casing drift, typo bugs, and duplicated magic values across call sites.

## Static readonly lookups

A fixed lookup collection (list/dictionary/set used for checking or mapping) that doesn't change per-call should be declared once — `static readonly` at class scope — not rebuilt inside the method on every call. Use the simplest structure that gives the lookup one stable source of truth; don't build an abstraction around it beyond that.

## Enum trailing commas

Always give the last enum member a trailing comma:

```csharp
public enum PermissionType
{
    User,
    Role,
}
```

Adding a member later then only touches one new line instead of also modifying the previous line — the same "future change should produce the smallest diff" principle that drives the parameter-wrapping and line-breaking rules in [04-coding-rules.md#formatting](../04-coding-rules.md#formatting).

## Git diff quality is a design goal

The formatting rules across this doc and [04-coding-rules.md#formatting](../04-coding-rules.md#formatting) share one motivation: a future change should touch only the lines that actually changed. Adding a parameter adds one XML `<param>` line. Adding an enum value doesn't touch the previous line. Adding a property doesn't reformat its neighbors. Format proactively with that in mind, not only when a diff tool complains.

---

## NovaCore-specific rules live elsewhere

The following are **not** portable to another project — they depend on NovaCore's specific architecture (EF Core today, kept swappable; the Read/Write persistence-service split) and are documented, enforced, and kept current in [persistence-coding-conventions.md](persistence-coding-conventions.md), not here:

- Transaction boundary ownership (`IUnitOfWork.ExecuteTransactionAsync`, when it opens, who owns it) — [persistence-coding-conventions.md#write-service-responsibility](persistence-coding-conventions.md#write-service-responsibility) and [04-coding-rules.md#transaction-management](../04-coding-rules.md#transaction-management).
- `SaveChangesAsync` placement (Persistence-owned, never scattered through Application) — same sections.
- `DbContext` access going through the repository abstraction rather than being injected ad hoc into Persistence Services — [persistence-coding-conventions.md#repository-responsibility](persistence-coding-conventions.md#repository-responsibility) and [persistence-coding-conventions.md#what-not-to-do](persistence-coding-conventions.md#what-not-to-do).

Don't copy those three rules into a different project's standards doc as if they were general C# style — they're a consequence of *this* codebase's persistence architecture.
