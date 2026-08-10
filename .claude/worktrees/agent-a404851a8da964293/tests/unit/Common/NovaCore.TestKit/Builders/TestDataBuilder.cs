namespace NovaCore.TestKit.Builders;

/// <summary>
/// Base for fluent Test Data Builders. A concrete builder holds sensible defaults for every
/// constructor argument, exposes one <c>With*()</c> method per field to override just what a
/// test cares about, and implements <see cref="Build"/> to call the real production factory
/// (e.g. an aggregate's static <c>Create</c> method) - never bypass it via reflection or object
/// initializers, or the builder stops protecting the same invariants production code does.
/// </summary>
/// <example>
/// public sealed class ProductBuilder : TestDataBuilder&lt;Product&gt;
/// {
///     private string _name = "Test Product";
///     public ProductBuilder WithName(string name) { _name = name; return this; }
///     public override Product Build() => Product.Create(Guid.CreateVersion7(), ..., _name, ...);
/// }
/// </example>
public abstract class TestDataBuilder<T>
{
    public abstract T Build();

    public static implicit operator T(TestDataBuilder<T> builder) => builder.Build();
}
